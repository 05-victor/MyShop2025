-- Diagnostic Query: Check Store ID Assignment Status
-- Run this to debug why store_id is not being assigned

-- 1. Check if the trigger exists
SELECT 
    tgname AS trigger_name,
    tgtype AS trigger_type,
    proname AS function_name
FROM pg_trigger t
JOIN pg_proc p ON t.tgfoid = p.oid
WHERE tgrelid = 'user_roles'::regclass;

-- 2. Check if the sequence exists and its current value
SELECT 
    sequencename,
    last_value,
    max_value,
    is_cycled
FROM user_store_id_seq;

-- 3. Check user_roles table structure
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'user_roles';

-- 4. Check which users have SalesAgent role
SELECT 
    u.id,
    u.username,
    u.store_id,
    ur.roles_name,
    CASE 
        WHEN u.store_id IS NULL THEN '? NOT ASSIGNED'
        ELSE '? ASSIGNED: ' || u.store_id::text
    END as store_id_status
FROM users u
JOIN user_roles ur ON u.id = ur.users_id
WHERE ur.roles_name = 'SalesAgent'
ORDER BY u.created_at DESC;

-- 5. Manually test the trigger function
DO $$
DECLARE
    test_user_id UUID;
    test_store_id INTEGER;
BEGIN
    -- Find a SalesAgent user without store_id
    SELECT u.id INTO test_user_id
    FROM users u
    JOIN user_roles ur ON u.id = ur.users_id
    WHERE ur.roles_name = 'SalesAgent' 
    AND u.store_id IS NULL
    LIMIT 1;
    
    IF test_user_id IS NOT NULL THEN
        RAISE NOTICE 'Found user without store_id: %', test_user_id;
        
        -- Manually call the update that the trigger should do
        UPDATE users 
        SET store_id = nextval('user_store_id_seq')
        WHERE id = test_user_id 
        AND store_id IS NULL
        RETURNING store_id INTO test_store_id;
        
        RAISE NOTICE 'Manually assigned store_id: %', test_store_id;
    ELSE
        RAISE NOTICE 'No SalesAgent users found without store_id';
    END IF;
END $$;

-- 6. Check the result after manual assignment
SELECT 
    u.id,
    u.username,
    u.store_id,
    ur.roles_name
FROM users u
JOIN user_roles ur ON u.id = ur.users_id
WHERE ur.roles_name = 'SalesAgent'
ORDER BY u.created_at DESC
LIMIT 5;

-- 7. Enable trigger logging (requires superuser)
-- Uncomment if you have superuser access
/*
ALTER TABLE user_roles ENABLE TRIGGER trigger_assign_store_id_on_role;
SET client_min_messages TO DEBUG;
*/
