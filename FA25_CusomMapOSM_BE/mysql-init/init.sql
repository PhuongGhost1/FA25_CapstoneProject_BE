-- MySQL Initialization Script for imos
-- This script runs when MySQL container is first started

-- Create database if not exists
CREATE DATABASE IF NOT EXISTS `imos` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user and grant privileges
CREATE USER IF NOT EXISTS 'imosuser'@'%' IDENTIFIED BY 'imospassword';
GRANT ALL PRIVILEGES ON `imos`.* TO 'imosuser'@'%';

-- Grant privileges to root user from any host
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY '123456' WITH GRANT OPTION;

-- Flush privileges to apply changes
FLUSH PRIVILEGES;

-- Use the imos database
USE `imos`;

-- Optional: Create initial tables or insert seed data here
-- Example:
-- CREATE TABLE IF NOT EXISTS `test_table` (
--     `id` INT AUTO_INCREMENT PRIMARY KEY,
--     `name` VARCHAR(255) NOT NULL,
--     `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
-- );

-- Print success message
SELECT 'MySQL database initialization completed successfully!' as message;