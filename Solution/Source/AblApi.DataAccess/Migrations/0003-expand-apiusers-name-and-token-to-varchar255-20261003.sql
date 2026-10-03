-- Expand ApiUsers.Name and ApiUsers.Token from varchar(64) to varchar(255)

ALTER TABLE "ApiUsers"
    ALTER COLUMN "Name" TYPE varchar(255),
    ALTER COLUMN "Token" TYPE varchar(255);
