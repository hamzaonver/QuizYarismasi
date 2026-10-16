-- =====================================================================
--  QuizArena — Veritabanı şeması (SQLite)
--  Hafta 4 teslimi: tablolar, ilişkiler (FK), kısıtlar (CHECK/UNIQUE), indeksler
--
--  NOT: Uygulama bu şemayı EF Core migration'ı ile kendisi oluşturur
--  (src/QuizArena.Data/Migrations). Bu dosya, migration'ın ürettiği şemanın
--  okunabilir/elle yazılmış eşdeğeridir; ders sunumunda DDL'i anlatmak için.
--  EF'nin ürettiği birebir betik: docs/database/migration.sql (ilk-kurulum betiği üretir)
-- =====================================================================

PRAGMA foreign_keys = ON;  -- SQLite'ta yabancı anahtar kontrolü varsayılan kapalıdır (EF Core otomatik açar)

-- ---------------------------------------------------------------------
-- Kullanıcılar (öğretmen + öğrenci)
-- ---------------------------------------------------------------------
CREATE TABLE Users (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName         TEXT    NOT NULL CHECK (length(FullName) <= 100),
    Email            TEXT    NOT NULL CHECK (length(Email) <= 150),
    PasswordHash     TEXT    NOT NULL,                 -- BCrypt özeti; düz parola ASLA saklanmaz
    Role             INTEGER NOT NULL,
    FailedLoginCount INTEGER NOT NULL DEFAULT 0,
    LockoutEndUtc    TEXT    NULL,
    LastLoginAt      TEXT    NULL,
    CreatedAt        TEXT    NOT NULL,
    CONSTRAINT CK_Users_Role CHECK (Role IN (1, 2))    -- 1 = Öğretmen, 2 = Öğrenci
);
CREATE UNIQUE INDEX IX_Users_Email ON Users (Email);

-- ---------------------------------------------------------------------
-- Quizler: Öğretmen (1) -> Quiz (N)
-- ---------------------------------------------------------------------
CREATE TABLE Quizzes (
    Id                      INTEGER PRIMARY KEY AUTOINCREMENT,
    TeacherId               INTEGER NOT NULL,
    Title                   TEXT    NOT NULL,
    Description             TEXT    NULL,
    DefaultTimeLimitSeconds INTEGER NOT NULL DEFAULT 20,
    IsPublished             INTEGER NOT NULL DEFAULT 0,  -- SQLite'ta bool = 0/1
    CreatedAt               TEXT    NOT NULL,
    UpdatedAt               TEXT    NULL,
    CONSTRAINT FK_Quizzes_Users_TeacherId FOREIGN KEY (TeacherId) REFERENCES Users (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_Quizzes_DefaultTime CHECK (DefaultTimeLimitSeconds BETWEEN 5 AND 300)
);
CREATE INDEX IX_Quizzes_TeacherId ON Quizzes (TeacherId);

-- ---------------------------------------------------------------------
-- Sorular: Quiz (1) -> Soru (N)
-- ---------------------------------------------------------------------
CREATE TABLE Questions (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    QuizId           INTEGER NOT NULL,
    Text             TEXT    NOT NULL,
    OrderNo          INTEGER NOT NULL,
    TimeLimitSeconds INTEGER NOT NULL DEFAULT 20,
    Points           INTEGER NOT NULL DEFAULT 1000,
    CreatedAt        TEXT    NOT NULL,
    CONSTRAINT FK_Questions_Quizzes_QuizId FOREIGN KEY (QuizId) REFERENCES Quizzes (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Questions_TimeLimit CHECK (TimeLimitSeconds BETWEEN 5 AND 300),
    CONSTRAINT CK_Questions_Points    CHECK (Points BETWEEN 0 AND 5000)
);
CREATE INDEX IX_Questions_QuizId_OrderNo ON Questions (QuizId, OrderNo);

-- ---------------------------------------------------------------------
-- Şıklar: Soru (1) -> Şık (N). Doğru cevap IsCorrect = 1
-- ---------------------------------------------------------------------
CREATE TABLE AnswerOptions (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    QuestionId INTEGER NOT NULL,
    Text       TEXT    NOT NULL,
    OrderNo    INTEGER NOT NULL,
    IsCorrect  INTEGER NOT NULL DEFAULT 0,
    CreatedAt  TEXT    NOT NULL,
    CONSTRAINT FK_AnswerOptions_Questions_QuestionId FOREIGN KEY (QuestionId) REFERENCES Questions (Id) ON DELETE CASCADE
);
CREATE INDEX IX_AnswerOptions_QuestionId_OrderNo ON AnswerOptions (QuestionId, OrderNo);

-- ---------------------------------------------------------------------
-- Canlı oturumlar: Quiz (1) -> Oturum (N), Öğretmen (1) -> Oturum (N)
-- ---------------------------------------------------------------------
CREATE TABLE QuizSessions (
    Id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    QuizId               INTEGER NOT NULL,
    HostId               INTEGER NOT NULL,
    JoinCode             TEXT    NOT NULL,
    Status               INTEGER NOT NULL DEFAULT 0,    -- 0 Bekliyor, 1 Aktif, 2 Bitti
    CurrentQuestionIndex INTEGER NOT NULL DEFAULT -1,
    StartedAt            TEXT    NULL,
    EndedAt              TEXT    NULL,
    CreatedAt            TEXT    NOT NULL,
    CONSTRAINT FK_QuizSessions_Quizzes_QuizId FOREIGN KEY (QuizId) REFERENCES Quizzes (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_QuizSessions_Users_HostId  FOREIGN KEY (HostId) REFERENCES Users (Id)   ON DELETE RESTRICT
);
CREATE UNIQUE INDEX IX_QuizSessions_JoinCode ON QuizSessions (JoinCode);
CREATE INDEX IX_QuizSessions_QuizId ON QuizSessions (QuizId);
CREATE INDEX IX_QuizSessions_HostId ON QuizSessions (HostId);

-- ---------------------------------------------------------------------
-- Katılımcılar: Oturum <-> Kullanıcı N-N ilişkisinin ara tablosu
-- ---------------------------------------------------------------------
CREATE TABLE Participants (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    SessionId  INTEGER NOT NULL,
    UserId     INTEGER NULL,                               -- misafir katılımcıda NULL
    Nickname   TEXT    NOT NULL,
    TotalScore INTEGER NOT NULL DEFAULT 0,
    JoinedAt   TEXT    NOT NULL,
    CreatedAt  TEXT    NOT NULL,
    CONSTRAINT FK_Participants_QuizSessions_SessionId FOREIGN KEY (SessionId) REFERENCES QuizSessions (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Participants_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE SET NULL
);
CREATE UNIQUE INDEX IX_Participants_SessionId_Nickname ON Participants (SessionId, Nickname);
CREATE INDEX IX_Participants_UserId ON Participants (UserId);

-- ---------------------------------------------------------------------
-- Cevaplar: Katılımcı <-> Soru N-N ilişkisinin ara tablosu
-- ---------------------------------------------------------------------
CREATE TABLE ParticipantAnswers (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    ParticipantId    INTEGER NOT NULL,
    QuestionId       INTEGER NOT NULL,
    SelectedOptionId INTEGER NULL,                         -- süre dolduysa NULL
    IsCorrect        INTEGER NOT NULL DEFAULT 0,
    ResponseTimeMs   INTEGER NOT NULL,
    PointsAwarded    INTEGER NOT NULL DEFAULT 0,
    AnsweredAt       TEXT    NOT NULL,
    CreatedAt        TEXT    NOT NULL,
    CONSTRAINT FK_ParticipantAnswers_Participants_ParticipantId FOREIGN KEY (ParticipantId) REFERENCES Participants (Id) ON DELETE CASCADE,
    CONSTRAINT FK_ParticipantAnswers_Questions_QuestionId FOREIGN KEY (QuestionId) REFERENCES Questions (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_ParticipantAnswers_AnswerOptions_SelectedOptionId FOREIGN KEY (SelectedOptionId) REFERENCES AnswerOptions (Id) ON DELETE RESTRICT
);
CREATE UNIQUE INDEX IX_ParticipantAnswers_ParticipantId_QuestionId ON ParticipantAnswers (ParticipantId, QuestionId);
CREATE INDEX IX_ParticipantAnswers_QuestionId ON ParticipantAnswers (QuestionId);
CREATE INDEX IX_ParticipantAnswers_SelectedOptionId ON ParticipantAnswers (SelectedOptionId);
