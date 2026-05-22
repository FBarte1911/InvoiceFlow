using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecurityEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Index required for RLS policy performance on PaymentReminders (security-rls-performance)
            migrationBuilder.CreateIndex(
                name: "IX_PaymentReminders_TenantId",
                table: "PaymentReminders",
                column: "TenantId");

            // Enable and force RLS on all tenant-scoped tables (security-rls-basics)
            migrationBuilder.Sql("""
                ALTER TABLE "Clients"         ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "Clients"         FORCE  ROW LEVEL SECURITY;
                ALTER TABLE "Invoices"        ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "Invoices"        FORCE  ROW LEVEL SECURITY;
                ALTER TABLE "InvoiceItems"    ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "InvoiceItems"    FORCE  ROW LEVEL SECURITY;
                ALTER TABLE "Subscriptions"   ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "Subscriptions"   FORCE  ROW LEVEL SECURITY;
                ALTER TABLE "PaymentReminders" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "PaymentReminders" FORCE  ROW LEVEL SECURITY;
                """);

            // RLS policies — SELECT wrapper caches current_setting per statement (security-rls-performance)
            migrationBuilder.Sql("""
                CREATE POLICY tenants_clients_policy ON "Clients"
                  FOR ALL
                  USING ("TenantId" = (SELECT current_setting('app.current_tenant_id', true)));

                CREATE POLICY tenants_invoices_policy ON "Invoices"
                  FOR ALL
                  USING ("TenantId" = (SELECT current_setting('app.current_tenant_id', true)));

                CREATE POLICY tenants_invoice_items_policy ON "InvoiceItems"
                  FOR ALL
                  USING (
                    EXISTS (
                      SELECT 1 FROM "Invoices" i
                      WHERE i."Id" = "InvoiceId"
                        AND i."TenantId" = (SELECT current_setting('app.current_tenant_id', true))
                    )
                  );

                CREATE POLICY tenants_subscriptions_policy ON "Subscriptions"
                  FOR ALL
                  USING ("TenantId" = (SELECT current_setting('app.current_tenant_id', true)));

                CREATE POLICY tenants_payment_reminders_policy ON "PaymentReminders"
                  FOR ALL
                  USING ("TenantId" = (SELECT current_setting('app.current_tenant_id', true)));
                """);

            // Least-privilege application role (security-privileges)
            // HTTP requests should connect as invoiceflow_app; postgres/superuser used only for migrations and background jobs
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'invoiceflow_app') THEN
                    CREATE ROLE invoiceflow_app NOLOGIN;
                  END IF;
                END
                $$;

                GRANT USAGE ON SCHEMA public TO invoiceflow_app;
                GRANT SELECT, INSERT, UPDATE, DELETE
                  ON "Clients", "Invoices", "InvoiceItems", "Subscriptions", "PaymentReminders"
                  TO invoiceflow_app;

                REVOKE ALL ON SCHEMA public FROM public;
                REVOKE ALL ON ALL TABLES IN SCHEMA public FROM public;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentReminders_TenantId",
                table: "PaymentReminders");

            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS tenants_clients_policy          ON "Clients";
                DROP POLICY IF EXISTS tenants_invoices_policy         ON "Invoices";
                DROP POLICY IF EXISTS tenants_invoice_items_policy    ON "InvoiceItems";
                DROP POLICY IF EXISTS tenants_subscriptions_policy    ON "Subscriptions";
                DROP POLICY IF EXISTS tenants_payment_reminders_policy ON "PaymentReminders";

                ALTER TABLE "Clients"          DISABLE ROW LEVEL SECURITY;
                ALTER TABLE "Invoices"         DISABLE ROW LEVEL SECURITY;
                ALTER TABLE "InvoiceItems"     DISABLE ROW LEVEL SECURITY;
                ALTER TABLE "Subscriptions"    DISABLE ROW LEVEL SECURITY;
                ALTER TABLE "PaymentReminders" DISABLE ROW LEVEL SECURITY;
                """);
        }
    }
}
