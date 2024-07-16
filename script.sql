-- Create Database
	CREATE DATABASE "MotoRentalDB";

-- Connect 
--	\c "MotoRentalDB"

-- Create Tables
	CREATE TABLE IF NOT EXISTS "Motos" (
		"Id" UUID PRIMARY KEY,
		"Year" INT NOT NULL,
		"Model" VARCHAR(100) NOT NULL,
		"Plate" VARCHAR(20) NOT NULL, 
		-- Plate is not unique due to logical exclusion
		"Deleted" BOOLEAN NOT NULL
	);

	CREATE INDEX idx_plate ON "Motos" ("Plate");

	CREATE TABLE IF NOT EXISTS "DeliveryPeople" (
		"Id" UUID PRIMARY KEY,
		"Name" VARCHAR(100) NOT NULL,
		"Cnpj" VARCHAR(14) NOT NULL UNIQUE,
		"BirthDate" DATE,
		"CnhNumber" VARCHAR(20) NOT NULL UNIQUE,
		"CnhType" VARCHAR(2) NOT NULL,
		"CnhImageUrl" VARCHAR(255) NOT NULL
	);

	CREATE TABLE IF NOT EXISTS "Rentals" (
		"Id" UUID PRIMARY KEY,
		"MotoId" UUID NOT NULL,
		"DeliveryPersonId" UUID NOT NULL,
		"StartDate" TIMESTAMP NOT NULL,
		"EndDate" TIMESTAMP NOT NULL,
		
		FOREIGN KEY ("MotoId") REFERENCES "Motos"("Id"),
		FOREIGN KEY ("DeliveryPersonId") REFERENCES "DeliveryPeople"("Id")
	);
