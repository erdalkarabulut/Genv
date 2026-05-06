SELECT COUNT(*) AS total, MIN("CreatedDate") AS min_created, MIN("UpdatedDate") AS min_updated, MIN("DeletedDate") AS min_deleted FROM "OperationClaims";
SELECT COUNT(*) AS created_lt_min FROM "OperationClaims" WHERE "CreatedDate" < '0001-01-01 00:00:00+00'::timestamptz;
SELECT COUNT(*) AS updated_lt_min FROM "OperationClaims" WHERE "UpdatedDate" IS NOT NULL AND "UpdatedDate" < '0001-01-01 00:00:00+00'::timestamptz;
SELECT COUNT(*) AS deleted_lt_min FROM "OperationClaims" WHERE "DeletedDate" IS NOT NULL AND "DeletedDate" < '0001-01-01 00:00:00+00'::timestamptz;
