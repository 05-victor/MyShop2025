using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add store_id column to users table
            migrationBuilder.AddColumn<int>(
                name: "store_id",
                table: "users",
                type: "integer",
                nullable: true);

            // Create a sequence for auto-incrementing store_id (starts at 0, max 99)
            migrationBuilder.Sql(@"
                CREATE SEQUENCE IF NOT EXISTS user_store_id_seq
                START WITH 0
                INCREMENT BY 1
                MINVALUE 0
                MAXVALUE 99
                CYCLE;
            ");

            // Create a function to auto-assign store_id for SalesAgent users
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION assign_store_id_to_sales_agent()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Check if the user has SalesAgent role and store_id is NULL
                    IF EXISTS (
                        SELECT 1 FROM user_roles ur
                        INNER JOIN roles r ON ur.roles_name = r.name
                        WHERE ur.users_id = NEW.id 
                        AND r.name = 'SalesAgent'
                    ) AND NEW.store_id IS NULL THEN
                        -- Assign next store_id from sequence
                        NEW.store_id := nextval('user_store_id_seq');
                    END IF;
                    
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger to call the function BEFORE INSERT on users table
            migrationBuilder.Sql(@"
                CREATE TRIGGER trigger_assign_store_id
                BEFORE INSERT ON users
                FOR EACH ROW
                EXECUTE FUNCTION assign_store_id_to_sales_agent();
            ");

            // Also create a trigger for when a role is assigned to an existing user
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION assign_store_id_on_role_change()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Check if SalesAgent role is being assigned
                    IF NEW.roles_name = 'SalesAgent' THEN
                        -- Update user's store_id if it's currently NULL
                        UPDATE users 
                        SET store_id = nextval('user_store_id_seq')
                        WHERE id = NEW.users_id 
                        AND store_id IS NULL;
                    END IF;
                    
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trigger_assign_store_id_on_role
                AFTER INSERT ON user_roles
                FOR EACH ROW
                EXECUTE FUNCTION assign_store_id_on_role_change();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop triggers first
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trigger_assign_store_id ON users;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trigger_assign_store_id_on_role ON user_roles;");
            
            // Drop functions
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS assign_store_id_to_sales_agent();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS assign_store_id_on_role_change();");
            
            // Drop sequence
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS user_store_id_seq;");
            
            // Drop column
            migrationBuilder.DropColumn(
                name: "store_id",
                table: "users");
        }
    }
}
