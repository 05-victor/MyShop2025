using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert Product status: text -> int
            // Mapping: "AVAILABLE" -> 0, "OUT_OF_STOCK" -> 1, "DISCONTINUED" -> 2, "PENDING" -> 3
            migrationBuilder.Sql(@"
                ALTER TABLE products ADD COLUMN status_temp integer;
                UPDATE products SET status_temp = 
                    CASE UPPER(TRIM(status))
                        WHEN 'AVAILABLE' THEN 0
                        WHEN 'OUTOFSTOCK' THEN 1
                        WHEN 'OUT_OF_STOCK' THEN 1
                        WHEN 'DISCONTINUED' THEN 2
                        WHEN 'PENDING' THEN 3
                        ELSE 0
                    END;
                ALTER TABLE products DROP COLUMN status;
                ALTER TABLE products RENAME COLUMN status_temp TO status;
                ALTER TABLE products ALTER COLUMN status SET DEFAULT 0;
                ALTER TABLE products ALTER COLUMN status SET NOT NULL;
            ");

            // Convert Order status: text -> int
            // Mapping: "PENDING" -> 0, "CONFIRMED" -> 1, "PROCESSING" -> 2, "SHIPPED" -> 3, "DELIVERED" -> 4, "CANCELLED" -> 5, "RETURNED" -> 6
            migrationBuilder.Sql(@"
                ALTER TABLE orders ADD COLUMN status_temp integer;
                UPDATE orders SET status_temp = 
                    CASE UPPER(TRIM(status))
                        WHEN 'PENDING' THEN 0
                        WHEN 'CONFIRMED' THEN 1
                        WHEN 'PROCESSING' THEN 2
                        WHEN 'SHIPPED' THEN 3
                        WHEN 'DELIVERED' THEN 4
                        WHEN 'CANCELLED' THEN 5
                        WHEN 'RETURNED' THEN 6
                        ELSE 0
                    END;
                ALTER TABLE orders DROP COLUMN status;
                ALTER TABLE orders RENAME COLUMN status_temp TO status;
                ALTER TABLE orders ALTER COLUMN status SET NOT NULL;
            ");

            // Convert Order payment_status: text -> int
            // Mapping: "UNPAID" -> 0, "PAID" -> 1, "PARTIALLY_PAID" -> 2, "REFUNDED" -> 3, "FAILED" -> 4
            migrationBuilder.Sql(@"
                ALTER TABLE orders ADD COLUMN payment_status_temp integer;
                UPDATE orders SET payment_status_temp = 
                    CASE UPPER(TRIM(payment_status))
                        WHEN 'UNPAID' THEN 0
                        WHEN 'PAID' THEN 1
                        WHEN 'PARTIALLYPAID' THEN 2
                        WHEN 'PARTIALLY_PAID' THEN 2
                        WHEN 'REFUNDED' THEN 3
                        WHEN 'FAILED' THEN 4
                        ELSE 0
                    END;
                ALTER TABLE orders DROP COLUMN payment_status;
                ALTER TABLE orders RENAME COLUMN payment_status_temp TO payment_status;
                ALTER TABLE orders ALTER COLUMN payment_status SET NOT NULL;
            ");

            // Convert AgentRequest status: text -> int
            // Mapping: "PENDING" -> 0, "APPROVED" -> 1, "REJECTED" -> 2
            migrationBuilder.Sql(@"
                ALTER TABLE agent_requests ADD COLUMN status_temp integer;
                UPDATE agent_requests SET status_temp = 
                    CASE UPPER(TRIM(status))
                        WHEN 'PENDING' THEN 0
                        WHEN 'APPROVED' THEN 1
                        WHEN 'REJECTED' THEN 2
                        ELSE 0
                    END;
                ALTER TABLE agent_requests DROP COLUMN status;
                ALTER TABLE agent_requests RENAME COLUMN status_temp TO status;
                ALTER TABLE agent_requests ALTER COLUMN status SET NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert Product status: int -> text
            migrationBuilder.Sql(@"
                ALTER TABLE products ADD COLUMN status_temp text;
                UPDATE products SET status_temp = 
                    CASE status
                        WHEN 0 THEN 'AVAILABLE'
                        WHEN 1 THEN 'OUT_OF_STOCK'
                        WHEN 2 THEN 'DISCONTINUED'
                        WHEN 3 THEN 'PENDING'
                        ELSE 'AVAILABLE'
                    END;
                ALTER TABLE products DROP COLUMN status;
                ALTER TABLE products RENAME COLUMN status_temp TO status;
            ");

            // Convert Order status: int -> text
            migrationBuilder.Sql(@"
                ALTER TABLE orders ADD COLUMN status_temp text;
                UPDATE orders SET status_temp = 
                    CASE status
                        WHEN 0 THEN 'PENDING'
                        WHEN 1 THEN 'CONFIRMED'
                        WHEN 2 THEN 'PROCESSING'
                        WHEN 3 THEN 'SHIPPED'
                        WHEN 4 THEN 'DELIVERED'
                        WHEN 5 THEN 'CANCELLED'
                        WHEN 6 THEN 'RETURNED'
                        ELSE 'PENDING'
                    END;
                ALTER TABLE orders DROP COLUMN status;
                ALTER TABLE orders RENAME COLUMN status_temp TO status;
                ALTER TABLE orders ALTER COLUMN status SET NOT NULL;
            ");

            // Convert Order payment_status: int -> text
            migrationBuilder.Sql(@"
                ALTER TABLE orders ADD COLUMN payment_status_temp text;
                UPDATE orders SET payment_status_temp = 
                    CASE payment_status
                        WHEN 0 THEN 'UNPAID'
                        WHEN 1 THEN 'PAID'
                        WHEN 2 THEN 'PARTIALLY_PAID'
                        WHEN 3 THEN 'REFUNDED'
                        WHEN 4 THEN 'FAILED'
                        ELSE 'UNPAID'
                    END;
                ALTER TABLE orders DROP COLUMN payment_status;
                ALTER TABLE orders RENAME COLUMN payment_status_temp TO payment_status;
                ALTER TABLE orders ALTER COLUMN payment_status SET NOT NULL;
            ");

            // Convert AgentRequest status: int -> text
            migrationBuilder.Sql(@"
                ALTER TABLE agent_requests ADD COLUMN status_temp character varying(20);
                UPDATE agent_requests SET status_temp = 
                    CASE status
                        WHEN 0 THEN 'Pending'
                        WHEN 1 THEN 'Approved'
                        WHEN 2 THEN 'Rejected'
                        ELSE 'Pending'
                    END;
                ALTER TABLE agent_requests DROP COLUMN status;
                ALTER TABLE agent_requests RENAME COLUMN status_temp TO status;
                ALTER TABLE agent_requests ALTER COLUMN status SET NOT NULL;
            ");
        }
    }
}
