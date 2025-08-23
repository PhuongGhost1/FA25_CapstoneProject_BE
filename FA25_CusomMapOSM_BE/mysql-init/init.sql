-- MySQL Initialization Script for CusomMapOSM
-- This script runs when MySQL container is first started

-- Create database if not exists
CREATE DATABASE IF NOT EXISTS `osm` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user and grant privileges
CREATE USER IF NOT EXISTS 'osmuser'@'%' IDENTIFIED BY 'osmpassword';
GRANT ALL PRIVILEGES ON `osm`.* TO 'osmuser'@'%';

-- Grant privileges to root user from any host
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY '123456' WITH GRANT OPTION;

-- Flush privileges to apply changes
FLUSH PRIVILEGES;

-- Use the osm database
USE `osm`;

-- Optional: Create initial tables or insert seed data here
-- Example:
-- CREATE TABLE IF NOT EXISTS `test_table` (
--     `id` INT AUTO_INCREMENT PRIMARY KEY,
--     `name` VARCHAR(255) NOT NULL,
--     `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
-- );

-- Print success message
SELECT 'MySQL database initialization completed successfully!' as message;