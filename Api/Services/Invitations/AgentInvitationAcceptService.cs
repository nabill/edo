﻿using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Invitations;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Infrastructure;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Invitations
{
    public class AgentInvitationAcceptService : IAgentInvitationAcceptService
    {
        public AgentInvitationAcceptService(EdoContext context, INotificationService notificationService,
            ILogger<AgentInvitationAcceptService> logger, IAdminAgencyManagementService agencyManagementService,
            Agents.IAgentService agentService, IInvitationRecordService invitationRecordService,
            IAccountManagementService accountManagementService, IAgencyService agencyService)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
            _agencyManagementService = agencyManagementService;
            _agentService = agentService;
            _agencyService = agencyService;
            _invitationRecordService = invitationRecordService;
            _accountManagementService = accountManagementService;
        }


        public async Task<Result> Accept(string invitationCode, UserInvitationData filledData, string identity, string email)
        {
            return await GetActiveInvitation()
                .Bind(Validate)
                .BindWithTransaction(_context, values => Result.Success(values)
                    .Tap(SaveAccepted)
                    .Bind(GetOrCreateAgency)
                    .Bind(CreateAgent)
                    .Tap(AddAgentAgencyRelation))
                .Tap(LogSuccess)
                .Tap(SendRegistrationMailToMaster)
                .OnFailure(LogFailed);


            async Task<Result<AcceptPipeValues>> GetActiveInvitation()
            {
                var (_, isFailure, invitation, error) = await _invitationRecordService.GetActiveInvitationByCode(invitationCode);
                if (isFailure)
                    return Result.Failure<AcceptPipeValues>(error);

                var originalInvitationData = _invitationRecordService.GetInvitationData(invitation);

                return new AcceptPipeValues
                {
                    Invitation = invitation,
                    InvitationData = filledData.Equals(default) ? originalInvitationData : filledData,
                    AgentRoleIds = originalInvitationData.RoleIds
                };
            }


            Task<Result<AcceptPipeValues>> Validate(AcceptPipeValues values)
                => Result.Success(values)
                    .Ensure(IsIdentityPresent, "User should have identity")
                    .Ensure(IsInvitationTypeCorrect, "Incorrect invitation type")
                    .Ensure(IsAgencyIdFilled, "Could not find inviter's agency id")
                    .Ensure(IsEmailFilled, "Agent email required")
                    .Ensure(IsAgentEmailUnique, "Agent with this email already exists");


            bool IsIdentityPresent(AcceptPipeValues _)
                => !string.IsNullOrWhiteSpace(identity);


            bool IsInvitationTypeCorrect(AcceptPipeValues values)
                => values.Invitation.InvitationType is UserInvitationTypes.Agent or UserInvitationTypes.ChildAgency;


            bool IsAgencyIdFilled(AcceptPipeValues values)
                => values.Invitation.InviterAgencyId.HasValue;


            bool IsEmailFilled(AcceptPipeValues _)
                => !string.IsNullOrWhiteSpace(email);


            async Task<bool> IsAgentEmailUnique(AcceptPipeValues _)
                => !await _context.Agents.AnyAsync(a => a.Email == email);

            Task SaveAccepted(AcceptPipeValues _)
                => _invitationRecordService.SetAccepted(invitationCode);


            async Task<Result<AcceptPipeValues>> CreateAgent(AcceptPipeValues values)
            {
                var (_, isFailure, agent, error) = await _agentService
                    .Add(values.InvitationData.UserRegistrationInfo, identity, email, values.Agency.PreferredCurrency);

                if (isFailure)
                    return Result.Failure<AcceptPipeValues>(error);

                values.Agent = agent;
                return values;
            }


            async Task<Result<AcceptPipeValues>> GetOrCreateAgency(AcceptPipeValues values)
            {
                var (_, isGetAgencyFailure, inviterAgency, agencyError) =
                    await _agencyManagementService.Get(values.Invitation.InviterAgencyId.Value);
                if (isGetAgencyFailure)
                    return Result.Failure<AcceptPipeValues>(agencyError);

                if (values.Invitation.InvitationType == UserInvitationTypes.Agent)
                {
                    values.Agency = inviterAgency;
                    values.RelationType = AgentAgencyRelationTypes.Regular;
                    values.NotificationType = NotificationTypes.AgentSuccessfulRegistration;

                    return values;
                }

                var (_, isValidationFailure, validationError) = AgencyValidator.Validate(values.InvitationData.ChildAgencyRegistrationInfo);
                if (isValidationFailure)
                    return Result.Failure<AcceptPipeValues>(validationError);

                var (_, isFailure, childAgency, error) = await _agencyService.Create(
                    values.InvitationData.ChildAgencyRegistrationInfo,
                    parentAgencyId: inviterAgency.Id);

                if (isFailure)
                    return Result.Failure<AcceptPipeValues>(error);

                var childAgencyRecord = await _context.Agencies.SingleAsync(a => a.Id == childAgency.Id.Value);
                await _accountManagementService.CreateForAgency(childAgencyRecord, childAgencyRecord.PreferredCurrency);

                values.AgencyName = childAgency.Name;
                values.Agency = childAgency;
                values.RelationType = AgentAgencyRelationTypes.Master;
                values.NotificationType = NotificationTypes.ChildAgencySuccessfulRegistration;

                return values;
            }


            Task AddAgentAgencyRelation(AcceptPipeValues values)
            {
                _context.AgentAgencyRelations.Add(new AgentAgencyRelation
                {
                    AgentId = values.Agent.Id,
                    Type = values.RelationType,
                    AgentRoleIds = values.AgentRoleIds,
                    AgencyId = values.Agency.Id!.Value,
                    IsActive = true
                });

                return _context.SaveChangesAsync();
            }


            void LogSuccess(AcceptPipeValues values)
                => _logger.LogAgentRegistrationSuccess(email);


            async Task SendRegistrationMailToMaster(AcceptPipeValues values)
            {
                var (_, isGetMasterFailure, master, getMasterError) = await _agentService.GetMasterAgent(values.Invitation.InviterAgencyId.Value);
                if (isGetMasterFailure)
                {
                    LogNotificationFailed(getMasterError);
                    return;
                }

                var registrationInfo = values.InvitationData.UserRegistrationInfo;
                var position = string.IsNullOrWhiteSpace(registrationInfo.Position)
                    ? "a new employee"
                    : registrationInfo.Position;

                var registrationData = new RegistrationData
                {
                    AgentName = $"{registrationInfo.FirstName} {registrationInfo.LastName}",
                    Position = position,
                    Title = registrationInfo.Title,
                    AgencyName = values.AgencyName
                };

                var (_, isNotificationFailure, notificationError)
                    = await _notificationService.Send(agent: new SlimAgentContext(master.Id, values.Invitation.InviterAgencyId.Value),
                        messageData: registrationData,
                        notificationType: values.NotificationType,
                        email: master.Email);

                if (isNotificationFailure)
                    LogNotificationFailed(notificationError);
            }


            void LogFailed(string error)
                => _logger.LogAgentRegistrationFailed(error);


            void LogNotificationFailed(string error)
                => _logger.LogAgentRegistrationNotificationFailure(error);
        }


        private struct AcceptPipeValues
        {
            public UserInvitation Invitation { get; set; }
            public UserInvitationData InvitationData { get; set; }
            public Agent Agent { get; set; }
            public AgencyInfo Agency { get; set; }
            public string AgencyName { get; set; }
            public int[] AgentRoleIds { get; set; }
            public AgentAgencyRelationTypes RelationType { get; set; }
            public NotificationTypes NotificationType { get; set; }
        }


        private readonly EdoContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AgentInvitationAcceptService> _logger;
        private readonly IAdminAgencyManagementService _agencyManagementService;
        private readonly Agents.IAgentService _agentService;
        private readonly IAgencyService _agencyService;
        private readonly IInvitationRecordService _invitationRecordService;
        private readonly IAccountManagementService _accountManagementService;
    }
}