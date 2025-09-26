-- Enable Row Level Security on tenant-scoped tables
ALTER TABLE "UserTenantRoles" ENABLE ROW LEVEL SECURITY;

-- Create a function to get the current tenant path
CREATE OR REPLACE FUNCTION get_current_tenant_path() RETURNS ltree AS $$
BEGIN
    RETURN current_setting('app.current_tenant_path', true)::ltree;
EXCEPTION
    WHEN OTHERS THEN
        RETURN NULL;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Create RLS policy for UserTenantRoles
CREATE POLICY tenant_isolation_policy ON "UserTenantRoles"
    FOR ALL
    USING (
        get_current_tenant_path() IS NULL OR
        "TenantPath" <@ get_current_tenant_path() OR
        get_current_tenant_path() <@ "TenantPath"
    );

-- Create policy for super admin bypass (optional)
CREATE POLICY bypass_rls_policy ON "UserTenantRoles"
    FOR ALL
    USING (current_setting('app.bypass_rls', true)::boolean = true);

-- Note: Add similar policies for other tenant-scoped tables as they are created