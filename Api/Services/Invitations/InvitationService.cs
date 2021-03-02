using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Invitations
{
    public class InvitationService : IInvitationService
    {
        public InvitationService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            MailSenderWithCompanyInfo mailSender,
            ILogger<InvitationService> logger,
            IAgencyManagementService agencyManagementService,
            IOptions<InvitationOptions> options,
            IManagementAuditService managementAuditService,
            Agents.IAgentService agentService,
            IOptions<AgentRegistrationNotificationOptions> notificationOptions)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _mailSender = mailSender;
            _logger = logger;
            _agencyManagementService = agencyManagementService;
            _options = options.Value;
            _managementAuditService = managementAuditService;
            _agentService = agentService;
            _notificationOptions = notificationOptions.Value;
        }


        public Task<Result<string>> Create(UserInvitationData prefilledData, UserInvitationTypes invitationType,
            bool shouldSendInvitationMail, int inviterUserId, int? inviterAgencyId = null)
        {
            var invitationCode = GenerateRandomCode();
            var now = _dateTimeProvider.UtcNow();

            return SaveInvitation()
                .CheckIf(shouldSendInvitationMail, SendInvitationMail)
                .Tap(LogInvitationCreated)
                .Map(_ => invitationCode);


            string GenerateRandomCode()
            {
                using var provider = new RNGCryptoServiceProvider();

                var byteArray = new byte[64];
                provider.GetBytes(byteArray);

                return Base64UrlEncoder.Encode(byteArray);
            }


            async Task<Result<UserInvitation>> SaveInvitation()
            {
                var newInvitation = new UserInvitation
                {
                    CodeHash = HashGenerator.ComputeSha256(invitationCode),
                    Email = prefilledData.AgentRegistrationInfo.Email,
                    Created = now,
                    InviterUserId = inviterUserId,
                    InviterAgencyId = inviterAgencyId,
                    InvitationType = invitationType,
                    InvitationStatus = UserInvitationStatuses.Active,
                    Data = JsonConvert.SerializeObject(prefilledData)
                };

                _context.UserInvitations.Add(newInvitation);

                await _context.SaveChangesAsync();

                return newInvitation;
            }


            async Task<Result> SendInvitationMail(UserInvitation newInvitation)
            {
                string agencyName = null;
                if (inviterAgencyId.HasValue)
                {
                    var getAgencyResult = await _agencyManagementService.Get(inviterAgencyId.Value);
                    if (getAgencyResult.IsFailure)
                        return Result.Failure("Could not find inviter agency");

                    agencyName = getAgencyResult.Value.Name;
                }

                var messagePayload = new InvitationData
                {
                    AgencyName = agencyName,
                    InvitationCode = invitationCode,
                    UserEmailAddress = prefilledData.AgentRegistrationInfo.Email,
                    UserName = $"{prefilledData.AgentRegistrationInfo.FirstName} {prefilledData.AgentRegistrationInfo.LastName}"
                };

                var templateId = GetTemplateId();
                if (string.IsNullOrWhiteSpace(templateId))
                    return Result.Failure("Could not find invitation mail template");

                return await _mailSender.Send(templateId,
                    prefilledData.AgentRegistrationInfo.Email,
                    messagePayload);
            }


            string GetTemplateId()
                => invitationType switch
                {
                    UserInvitationTypes.Agent => _options.AgentInvitationTemplateId,
                    UserInvitationTypes.Administrator => _options.AdminInvitationTemplateId,
                    UserInvitationTypes.ChildAgency => _options.ChildAgencyInvitationTemplateId,
                    _ => null
                };
            

            void LogInvitationCreated() => _logger.LogInvitationCreated(
                $"The invitation with type {invitationType} created for the user '{prefilledData.AgentRegistrationInfo.Email}'");
        }


        public async Task<Result> Disable(string code)
        {
            return await GetActiveInvitation(code)
                .Tap(SaveDisabled);


            Task SaveDisabled(UserInvitation invitation)
            {
                invitation.InvitationStatus = UserInvitationStatuses.Disabled;
                _context.Update(invitation);
                return _context.SaveChangesAsync();
            }
        }


        public Task<Result<string>> Resend(string code)
        {
            return GetActiveInvitation(code)
                .Bind(SendNewInvitation)
                .Tap(r => SaveResent(r.oldInvitation))
                .Map(r => r.newCode);


            async Task<Result<(UserInvitation oldInvitation, string newCode)>> SendNewInvitation(UserInvitation oldInvitation)
            {
                var (_, isFailure, newCode, error) = await Create(GetInvitationData(oldInvitation),
                    oldInvitation.InvitationType,
                    true,
                    oldInvitation.InviterUserId,
                    oldInvitation.InviterAgencyId);

                if (isFailure)
                    return Result.Failure<(UserInvitation, string)>(error);

                return (oldInvitation, newCode);
            }


            Task SaveResent(UserInvitation oldInvitation)
            {
                oldInvitation.InvitationStatus = UserInvitationStatuses.Resent;
                _context.Update(oldInvitation);
                return _context.SaveChangesAsync();
            }
        }


        public async Task<Result> Accept(string invitationCode, UserInvitationData filledData, string identity)
        {
            return await GetActiveInvitation(invitationCode)
                .Ensure(IsIdentityPresent, "User should have identity")
                .BindWithTransaction(_context, invitation => Result.Success(invitation)
                    .Check(RouteByInvitationType)
                    .Tap(SaveAccepted));


            bool IsIdentityPresent(UserInvitation _) => !string.IsNullOrWhiteSpace(identity);


            Task<Result> RouteByInvitationType(UserInvitation invitation)
            {
                return invitation.InvitationType switch
                {
                    UserInvitationTypes.Administrator => AdminInvitation(invitation),
                    UserInvitationTypes.Agent => AgentInvitation(invitation),
                    UserInvitationTypes.ChildAgency => AgentInvitation(invitation),
                    _ => throw new NotImplementedException($"{Formatters.EnumFormatters.FromDescription(invitation.InvitationType)} not supported")
                };
            }


            Task SaveAccepted(UserInvitation invitation)
            {
                invitation.InvitationStatus = UserInvitationStatuses.Accepted;
                _context.Update(invitation);
                return _context.SaveChangesAsync();
            }


            async Task<Result> AdminInvitation(UserInvitation invitation)
            {
                return await Result.Success()
                    .Map(CreateAdmin)
                    .Tap(WriteAuditLog);

                
                async Task<Administrator> CreateAdmin()
                {
                    var now = _dateTimeProvider.UtcNow();
                    var invitationData = GetData();

                    var administrator = new Administrator
                    {
                        Email = invitationData.AgentRegistrationInfo.Email,
                        FirstName = invitationData.AgentRegistrationInfo.FirstName,
                        LastName = invitationData.AgentRegistrationInfo.LastName,
                        IdentityHash = HashGenerator.ComputeSha256(identity),
                        Position = invitationData.AgentRegistrationInfo.Position,
                        Created = now,
                        Updated = now
                    };

                    _context.Administrators.Add(administrator);
                    await _context.SaveChangesAsync();

                    return administrator;
                }


                Task WriteAuditLog(Administrator administrator)
                    => _managementAuditService.Write(ManagementEventType.AdministratorRegistration,
                        new AdministrationRegistrationEvent(administrator.Email, administrator.Id, invitationCode));


                UserInvitationData GetData() => filledData.Equals(default) ? GetInvitationData(invitation) : filledData;
            }


            async Task<Result> AgentInvitation(UserInvitation invitation)
            {
                var invitationData = GetData();

                return await Result.Success()
                    .Ensure(IsEmailFilled, "Agent email required")
                    .Ensure(IsAgentEmailUnique, "Agent with this email already exists")
                    .Ensure(() => invitation.InviterAgencyId.HasValue, "Could not find inviter's agency id")
                    .Bind(CreateAgent)
                    .Bind(CreateChildAgencyIfRequired)
                    .Tap(AddAgentAgencyRelation)
                    .Tap(LogSuccess)
                    .Tap(SendRegistrationMailToMaster)
                    .OnFailure(LogFailed);



                bool IsEmailFilled() => !string.IsNullOrWhiteSpace(invitationData.AgentRegistrationInfo.Email);


                async Task<bool> IsAgentEmailUnique() => !await _context.Agents.AnyAsync(a => a.Email == invitationData.AgentRegistrationInfo.Email);


                async Task<Result<Agent>> CreateAgent()
                {
                    var (_, isFailure, agent, error) = await _agentService.Add(invitationData.AgentRegistrationInfo, identity, invitationData.AgentRegistrationInfo.Email);
                    return isFailure
                        ? Result.Failure<Agent>(error)
                        : Result.Success(agent);
                }


                async Task<Result<(Agent agent, int agencyId, string agencyName)>> CreateChildAgencyIfRequired(Agent agent)
                {
                    if (invitation.InvitationType == UserInvitationTypes.Agent)
                        return (agent, invitation.InviterAgencyId.Value, null);

                    var (_, isGetAgencyFailure, inviterAgency, error) = await _agencyManagementService.Get(invitation.InviterAgencyId.Value);
                    if (isGetAgencyFailure)
                        return Result.Failure<(Agent, int, string)>(error);

                    var childAgency = await _agencyManagementService.Create(
                        invitationData.ChildAgencyRegistrationInfo.Name, inviterAgency.CounterpartyId.Value, inviterAgency.Id);

                    return (agent, childAgency.Id.Value, childAgency.Name);
                }
                

                Task AddAgentAgencyRelation((Agent agent, int agencyId, string agencyName) values)
                {
                    var relationProps =
                        invitation.InvitationType switch
                        {
                            UserInvitationTypes.Agent => (permissions: PermissionSets.Default, relationType: AgentAgencyRelationTypes.Regular),
                            UserInvitationTypes.ChildAgency => (permissions: PermissionSets.Master, relationType: AgentAgencyRelationTypes.Master)
                        };

                    _context.AgentAgencyRelations.Add(new AgentAgencyRelation
                    {
                        AgentId = values.agent.Id,
                        Type = relationProps.relationType,
                        InAgencyPermissions = relationProps.permissions,
                        AgencyId = values.agencyId,
                        IsActive = true
                    });

                    return _context.SaveChangesAsync();
                }


                void LogSuccess()
                    => _logger.LogAgentRegistrationSuccess($"Agent {invitationData.AgentRegistrationInfo.Email} successfully registered " +
                        $"and bound to agency ID:'{invitation.InviterAgencyId.Value}'");


                async Task SendRegistrationMailToMaster((Agent agent, int agencyId, string agencyName) values)
                {
                    var (_, isGetMasterFailure, master, getMasterError) = await _agentService.GetMasterAgent(invitation.InviterAgencyId.Value);
                    if (isGetMasterFailure)
                    {
                        LogNotificationFailed(getMasterError);
                        return;
                    }

                    var registrationInfo = invitationData.AgentRegistrationInfo;
                    var position = string.IsNullOrWhiteSpace(registrationInfo.Position)
                        ? "a new employee"
                        : registrationInfo.Position;

                    var (_, isNotificationFailure, notificationError) = await _mailSender.Send(GetNotificationTemplateId(), master.Email,
                        new RegistrationData
                        {
                            AgentName = $"{registrationInfo.FirstName} {registrationInfo.LastName}",
                            Position = position,
                            Title = registrationInfo.Title,
                            AgencyName = values.agencyName
                        });

                    if (isNotificationFailure)
                        LogNotificationFailed(notificationError);
                }


                void LogFailed(string error) => _logger.LogAgentRegistrationFailed(error);


                void LogNotificationFailed(string error) => _logger.LogAgentRegistrationNotificationFailure(error);


                string GetNotificationTemplateId()
                    => invitation.InvitationType switch
                    {
                        UserInvitationTypes.Agent => _notificationOptions.RegularAgentMailTemplateId,
                        UserInvitationTypes.ChildAgency => _notificationOptions.ChildAgencyMailTemplateId
                    };


                UserInvitationData GetData() => filledData.Equals(default) ? GetInvitationData(invitation) : filledData;
            }
        }
        

        public Task<List<AgentInvitationResponse>> GetAgentAcceptedInvitations(int agentId)
            => GetInvitationsWithInviter(i => i.InviterUserId == agentId && i.InvitationStatus == UserInvitationStatuses.Accepted);


        public Task<List<AgentInvitationResponse>> GetAgentNotAcceptedInvitations(int agentId)
            => GetInvitationsWithInviter(i => i.InviterUserId == agentId && i.InvitationStatus != UserInvitationStatuses.Accepted);


        public Task<List<AgentInvitationResponse>> GetAgencyAcceptedInvitations(int agencyId)
            => GetInvitationsWithInviter(i => i.InviterAgencyId == agencyId && i.InvitationStatus == UserInvitationStatuses.Accepted);


        public Task<List<AgentInvitationResponse>> GetAgencyNotAcceptedInvitations(int agencyId)
            => GetInvitationsWithInviter(i => i.InviterAgencyId == agencyId && i.InvitationStatus != UserInvitationStatuses.Accepted);


        public Task<Result<UserInvitation>> GetActiveInvitation(string code)
        {
            return GetInvitation()
                .Ensure(InvitationIsActual, "Invitation expired");


            async Task<Result<UserInvitation>> GetInvitation()
            {
                var invitation = await _context.UserInvitations
                    .SingleOrDefaultAsync(i
                        => i.CodeHash == HashGenerator.ComputeSha256(code)
                        && i.InvitationStatus == UserInvitationStatuses.Active);

                return invitation ?? Result.Failure<UserInvitation>("Invitation with specified code either does not exist, or is not active.");
            }


            bool InvitationIsActual(UserInvitation invitation) => invitation.Created + _options.InvitationExpirationPeriod > _dateTimeProvider.UtcNow();
        }


        private async Task<List<AgentInvitationResponse>> GetInvitationsWithInviter(Expression<Func<UserInvitation, bool>> filterExpression)
        {
            var rows = await _context
                .UserInvitations
                .Where(i => i.InvitationStatus != UserInvitationStatuses.Resent)
                .Where(filterExpression)
                .Join(
                    _context.Agents,
                    invite => invite.InviterUserId,
                    agent => agent.Id,
                    (invite, inviter) => new { Invite = invite, Inviter = inviter }
                )
                .ToListAsync();

            return rows.Select(r => new {r.Invite, r.Inviter, Data = GetInvitationData(r.Invite)})
                .Select(i => new AgentInvitationResponse(
                    i.Invite.CodeHash,
                    i.Data.AgentRegistrationInfo.Title,
                    i.Data.AgentRegistrationInfo.FirstName,
                    i.Data.AgentRegistrationInfo.LastName,
                    i.Data.AgentRegistrationInfo.Position,
                    i.Invite.Email,
                    $"{i.Inviter.FirstName} {i.Inviter.LastName}",
                    DateTimeFormatters.ToDateString(i.Invite.Created))
                )
                .ToList();
        }


        private UserInvitationData GetInvitationData(UserInvitation invitation)
            => JsonConvert.DeserializeObject<UserInvitationData>(invitation.Data);


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly ILogger<InvitationService> _logger;
        private readonly IAgencyManagementService _agencyManagementService;
        private readonly InvitationOptions _options;
        private readonly IManagementAuditService _managementAuditService;
        private readonly Agents.IAgentService _agentService;
        private readonly AgentRegistrationNotificationOptions _notificationOptions;
    }
}
