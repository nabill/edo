using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveFakeAge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("" +
                "do $$declare\n" +
                "  booking_rec record;\n" +
                "  all_rooms_json jsonb;\n" +
                "  new_all_rooms_json jsonb;\n" +
                "  room_counter integer;\n" +
                "  room_json jsonb;\n" +
                "  passenger_counter integer;\n" +
                "  passenger_json jsonb;\n" +
                "  counter integer = 0;\n" +
                "  modified bool;\n " +
                "begin\n" +
                "  for booking_rec in select \"Id\", \"Rooms\" from \"Bookings\" loop --go through bookings\n" +
                "  \n" +
                "    modified = false;\n" +
                "    all_rooms_json = booking_rec.\"Rooms\";\n" +
                "    new_all_rooms_json = all_rooms_json;\n" +
                "    \n" +
                "    for room_counter in 0..jsonb_array_length(all_rooms_json)-1 loop --go through rooms\n" +
                "    \n" +
                "      room_json = all_rooms_json #> array[room_counter::text];\n" +
                "      \n" +
                "      if jsonb_typeof(room_json #> '{passengers}') = 'array' then \n" +
                "        for passenger_counter in 0..jsonb_array_length(room_json #> '{passengers}')-1 loop --go through passengers\n" +
                "        \n" +
                "          passenger_json = room_json #> array['passengers', passenger_counter::text];\n" +
                "          \n" +
                "          if (passenger_json #>> '{age}')::integer > 30 then\n" +
                "            counter = counter + 1;\n" +
                "            modified = true;\n" +
                "            new_all_rooms_json = jsonb_set(new_all_rooms_json,\n" +
                "                                           array[room_counter::text, 'passengers', passenger_counter::text, 'age'], jsonb 'null');\n" +
                "            \n" +
                "            raise notice 'booking % -> room % -> passenger %, age %'\n" +
                "            , booking_rec.\"Id\"\n" +
                "            , room_counter\n" +
                "            , passenger_counter\n" +
                "            , all_rooms_json #> array[room_counter::text, 'passengers', passenger_counter::text];\n" +
                "          end if;\n" +
                "        end loop; --end passengers loop\n" +
                "      end if;\n" +
                "    end loop; --end rooms loop\n" +
                "    update \"Bookings\" set \"Rooms\" = new_all_rooms_json where \"Id\" = booking_rec.\"Id\";\n" +
                "  end loop; --end bookings loop\n" +
                "  \n" +
                "  raise notice 'total %', counter;\n " +
                "end$$\n");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
