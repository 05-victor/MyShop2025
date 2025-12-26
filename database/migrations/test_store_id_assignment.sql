-- ============================================
-- Test Script for Store ID Auto-Assignment
-- ============================================
-- This script tests the automatic assignment of store_id to SalesAgent users

-- ============================================
-- Test 1: Create a new SalesAgent user
-- ============================================
DO $$
DECLARE
    test_user_id UUID;
BEGIN
    -- Generate a new user ID
    test_user_id := gen_random_uuid();
    
    -- Insert new user
    INSERT INTO users (id, username, email, password, created_at, is_trial_active, is_email_verified)
    VALUES (
        test_user_id,
        'test_salesagent_1',
        'testagent1@example.com',
        'hashed_password_here',
        NOW(),
        false,
        false
    );
    
    -- Assign SalesAgent role
    INSERT INTO user_roles (users_id, roles_name)
    VALUES (test_user_id, 'SalesAgent');
    
    -- Display result
    RAISE NOTICE 'Test 1: New SalesAgent user created';
    RAISE NOTICE 'User ID: %', test_user_id;
END $$;

-- Verify Test 1 result
SELECT 
    u.id,
    u.username,
    u.store_id,
    string_agg(r.name, ', ') as roles
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.users_id
LEFT JOIN roles r ON ur.roles_name = r.name
WHERE u.username = 'test_salesagent_1'
GROUP BY u.id, u.username, u.store_id;

-- Expected: store_id should be assigned (e.g., 1, 2, 3...)

-- ============================================
-- Test 2: Create a Customer (no store_id)
-- ============================================
DO $$
DECLARE
    test_user_id UUID;
BEGIN
    test_user_id := gen_random_uuid();
    
    INSERT INTO users (id, username, email, password, created_at, is_trial_active, is_email_verified)
    VALUES (
        test_user_id,
        'test_customer_1',
        'testcustomer1@example.com',
        'hashed_password_here',
        NOW(),
        false,
        false
    );
    
    -- Don't assign any role (or assign Customer role if it exists)
    -- No SalesAgent role = no store_id
    
    RAISE NOTICE 'Test 2: Customer user created';
    RAISE NOTICE 'User ID: %', test_user_id;
END $$;

-- Verify Test 2 result
SELECT 
    u.id,
    u.username,
    u.store_id,
    CASE WHEN u.store_id IS NULL THEN 'NULL (correct)' ELSE 'ERROR: Should be NULL' END as store_id_status
FROM users u
WHERE u.username = 'test_customer_1';

-- Expected: store_id should be NULL

-- ============================================
-- Test 3: Upgrade existing user to SalesAgent
-- ============================================
DO $$
DECLARE
    test_user_id UUID;
BEGIN
    test_user_id := gen_random_uuid();
    
    -- Create user without role first
    INSERT INTO users (id, username, email, password, created_at, is_trial_active, is_email_verified)
    VALUES (
        test_user_id,
        'test_upgrade_user',
        'testupgrade@example.com',
        'hashed_password_here',
        NOW(),
        false,
        false
    );
    
    -- Wait a moment then assign SalesAgent role
    INSERT INTO user_roles (users_id, roles_name)
    VALUES (test_user_id, 'SalesAgent');
    
    RAISE NOTICE 'Test 3: User upgraded to SalesAgent';
    RAISE NOTICE 'User ID: %', test_user_id;
END $$;

-- Verify Test 3 result
SELECT 
    u.id,
    u.username,
    u.store_id,
    string_agg(r.name, ', ') as roles
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.users_id
LEFT JOIN roles r ON ur.roles_name = r.name
WHERE u.username = 'test_upgrade_user'
GROUP BY u.id, u.username, u.store_id;

-- Expected: store_id should be assigned

-- ============================================
-- Test 4: Manual store_id override
-- ============================================
DO $$
DECLARE
    test_user_id UUID;
BEGIN
    test_user_id := gen_random_uuid();
    
    -- Create user with pre-set store_id
    INSERT INTO users (id, username, email, password, store_id, created_at, is_trial_active, is_email_verified)
    VALUES (
        test_user_id,
        'test_manual_store',
        'testmanual@example.com',
        'hashed_password_here',
        42,  -- Manually set store_id
        NOW(),
        false,
        false
    );
    
    -- Assign SalesAgent role
    INSERT INTO user_roles (users_id, roles_name)
    VALUES (test_user_id, 'SalesAgent');
    
    RAISE NOTICE 'Test 4: User with manual store_id';
    RAISE NOTICE 'User ID: %', test_user_id;
END $$;

-- Verify Test 4 result
SELECT 
    u.id,
    u.username,
    u.store_id,
    CASE WHEN u.store_id = 42 THEN 'Correct (42)' ELSE 'ERROR: Should be 42' END as store_id_status
FROM users u
WHERE u.username = 'test_manual_store';

-- Expected: store_id should remain 42 (not overwritten)

-- ============================================
-- Summary: Check all test users
-- ============================================
SELECT 
    u.username,
    u.store_id,
    string_agg(r.name, ', ') as roles,
    u.created_at
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.users_id
LEFT JOIN roles r ON ur.roles_name = r.name
WHERE u.username LIKE 'test_%'
GROUP BY u.username, u.store_id, u.created_at
ORDER BY u.created_at;

-- ============================================
-- Check sequence status
-- ============================================
SELECT 
    last_value as current_store_id,
    max_value,
    is_cycled
FROM user_store_id_seq;

-- ============================================
-- Cleanup: Remove test users (optional)
-- ============================================
/*
DELETE FROM users WHERE username LIKE 'test_%';
-- This will cascade delete user_roles entries
*/

-- ============================================
-- End of Test Script
-- ============================================
