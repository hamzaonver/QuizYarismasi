CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "FullName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" INTEGER NOT NULL,
    "FailedLoginCount" INTEGER NOT NULL,
    "LockoutEndUtc" TEXT NULL,
    "LastLoginAt" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "CK_Users_Role" CHECK ("Role" IN (1, 2))
);

CREATE TABLE "Quizzes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Quizzes" PRIMARY KEY AUTOINCREMENT,
    "TeacherId" INTEGER NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NULL,
    "DefaultTimeLimitSeconds" INTEGER NOT NULL,
    "IsPublished" INTEGER NOT NULL,
    "UpdatedAt" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "CK_Quizzes_DefaultTime" CHECK ("DefaultTimeLimitSeconds" BETWEEN 5 AND 300),
    CONSTRAINT "FK_Quizzes_Users_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Questions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Questions" PRIMARY KEY AUTOINCREMENT,
    "QuizId" INTEGER NOT NULL,
    "Text" TEXT NOT NULL,
    "OrderNo" INTEGER NOT NULL,
    "TimeLimitSeconds" INTEGER NOT NULL,
    "Points" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "CK_Questions_Points" CHECK ("Points" BETWEEN 0 AND 5000),
    CONSTRAINT "CK_Questions_TimeLimit" CHECK ("TimeLimitSeconds" BETWEEN 5 AND 300),
    CONSTRAINT "FK_Questions_Quizzes_QuizId" FOREIGN KEY ("QuizId") REFERENCES "Quizzes" ("Id") ON DELETE CASCADE
);

CREATE TABLE "QuizSessions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_QuizSessions" PRIMARY KEY AUTOINCREMENT,
    "QuizId" INTEGER NOT NULL,
    "HostId" INTEGER NOT NULL,
    "JoinCode" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "CurrentQuestionIndex" INTEGER NOT NULL,
    "StartedAt" TEXT NULL,
    "EndedAt" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_QuizSessions_Quizzes_QuizId" FOREIGN KEY ("QuizId") REFERENCES "Quizzes" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_QuizSessions_Users_HostId" FOREIGN KEY ("HostId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "AnswerOptions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AnswerOptions" PRIMARY KEY AUTOINCREMENT,
    "QuestionId" INTEGER NOT NULL,
    "Text" TEXT NOT NULL,
    "OrderNo" INTEGER NOT NULL,
    "IsCorrect" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_AnswerOptions_Questions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "Questions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Participants" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Participants" PRIMARY KEY AUTOINCREMENT,
    "SessionId" INTEGER NOT NULL,
    "UserId" INTEGER NULL,
    "Nickname" TEXT NOT NULL,
    "TotalScore" INTEGER NOT NULL,
    "JoinedAt" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_Participants_QuizSessions_SessionId" FOREIGN KEY ("SessionId") REFERENCES "QuizSessions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Participants_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE TABLE "ParticipantAnswers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ParticipantAnswers" PRIMARY KEY AUTOINCREMENT,
    "ParticipantId" INTEGER NOT NULL,
    "QuestionId" INTEGER NOT NULL,
    "SelectedOptionId" INTEGER NULL,
    "IsCorrect" INTEGER NOT NULL,
    "ResponseTimeMs" INTEGER NOT NULL,
    "PointsAwarded" INTEGER NOT NULL,
    "AnsweredAt" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_ParticipantAnswers_AnswerOptions_SelectedOptionId" FOREIGN KEY ("SelectedOptionId") REFERENCES "AnswerOptions" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ParticipantAnswers_Participants_ParticipantId" FOREIGN KEY ("ParticipantId") REFERENCES "Participants" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ParticipantAnswers_Questions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "Questions" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_AnswerOptions_QuestionId" ON "AnswerOptions" ("QuestionId");

CREATE UNIQUE INDEX "IX_ParticipantAnswers_ParticipantId_QuestionId" ON "ParticipantAnswers" ("ParticipantId", "QuestionId");

CREATE INDEX "IX_ParticipantAnswers_QuestionId" ON "ParticipantAnswers" ("QuestionId");

CREATE INDEX "IX_ParticipantAnswers_SelectedOptionId" ON "ParticipantAnswers" ("SelectedOptionId");

CREATE UNIQUE INDEX "IX_Participants_SessionId_Nickname" ON "Participants" ("SessionId", "Nickname");

CREATE INDEX "IX_Participants_UserId" ON "Participants" ("UserId");

CREATE INDEX "IX_Questions_QuizId_OrderNo" ON "Questions" ("QuizId", "OrderNo");

CREATE INDEX "IX_QuizSessions_HostId" ON "QuizSessions" ("HostId");

CREATE UNIQUE INDEX "IX_QuizSessions_JoinCode" ON "QuizSessions" ("JoinCode");

CREATE INDEX "IX_QuizSessions_QuizId" ON "QuizSessions" ("QuizId");

CREATE INDEX "IX_Quizzes_TeacherId" ON "Quizzes" ("TeacherId");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261015160221_InitialCreate', '8.0.11');

COMMIT;

