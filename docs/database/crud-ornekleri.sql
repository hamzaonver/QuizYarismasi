-- =====================================================================
--  QuizArena — Temel CRUD işlemlerinin SQL karşılıkları
--  Hafta 4: "Bir CRUD işleminin SQL'ini açıkla" sorusu için.
--  Uygulamada bu sorguları EF Core üretir (Repository'lerde LINQ olarak yazılı).
--  Geliştirme ortamında EF'nin ürettiği gerçek SQL'i konsol çıktısında görebilirsin
--  (appsettings.Development.json -> "Microsoft.EntityFrameworkCore.Database.Command": "Information").
-- =====================================================================

-- ---------- CREATE (Ekle) ----------
-- QuizService.CreateAsync -> QuizRepository.AddAsync + SaveAsync
INSERT INTO Quizzes (TeacherId, Title, Description, DefaultTimeLimitSeconds, IsPublished, CreatedAt)
VALUES (1, 'Python Temelleri', 'Deneme quizi', 20, 0, '2026-10-15 09:00:00');

-- Soru ve şıkları: EF Core, aynı SaveChanges içinde hepsini tek bir İŞLEM (transaction) olarak yazar.
-- Bir adım hata verirse (ör. CHECK kısıtı) hiçbiri kaydedilmez.
BEGIN TRANSACTION;
INSERT INTO Questions (QuizId, Text, OrderNo, TimeLimitSeconds, Points, CreatedAt)
VALUES (last_insert_rowid(), 'print() ne yapar?', 1, 20, 1000, '2026-10-15 09:01:00');
INSERT INTO AnswerOptions (QuestionId, Text, OrderNo, IsCorrect, CreatedAt) VALUES
    (last_insert_rowid(), 'Ekrana yazar', 1, 1, '2026-10-15 09:01:00');
COMMIT;

-- ---------- READ (Oku) ----------
-- QuizRepository.GetByTeacherAsync: öğretmenin quizleri, soru sayısıyla (IX_Quizzes_TeacherId kullanılır)
SELECT q.Id, q.Title, q.IsPublished, COUNT(qs.Id) AS QuestionCount
FROM Quizzes q
LEFT JOIN Questions qs ON qs.QuizId = q.Id
WHERE q.TeacherId = 1
GROUP BY q.Id, q.Title, q.IsPublished
ORDER BY q.CreatedAt DESC;

-- QuizRepository.GetWithQuestionsAsync: quiz + sorular + şıklar (JOIN)
SELECT q.Title, qs.OrderNo, qs.Text AS Question, o.OrderNo AS OptNo, o.Text AS OptionText, o.IsCorrect
FROM Quizzes q
JOIN Questions qs     ON qs.QuizId = q.Id
JOIN AnswerOptions o  ON o.QuestionId = qs.Id
WHERE q.Id = 1
ORDER BY qs.OrderNo, o.OrderNo;

-- UserRepository.GetByEmailAsync: girişte e-postaya göre arama (UNIQUE indeks -> tam tarama yok)
SELECT Id, FullName, Email, PasswordHash, Role FROM Users WHERE Email = 'ogretmen@quizarena.test' LIMIT 1;

-- ---------- UPDATE (Güncelle) ----------
-- QuizService.UpdateAsync
UPDATE Quizzes
SET Title = 'Python Temelleri (Güncel)', UpdatedAt = '2026-10-15 09:10:00'
WHERE Id = 1 AND TeacherId = 1;     -- Sahiplik koşulu: başkasının quizi güncellenemez

-- QuizService.SetPublishedAsync
UPDATE Quizzes SET IsPublished = 1 WHERE Id = 1;

-- ---------- DELETE (Sil) ----------
-- QuizService.DeleteQuestionAsync: şıklar ON DELETE CASCADE ile otomatik silinir
DELETE FROM Questions WHERE Id = 1;

-- QuizService.DeleteAsync: oturumu olan quiz silinmeye çalışılırsa
-- FK (ON DELETE RESTRICT) hatası verir -> "FOREIGN KEY constraint failed"
DELETE FROM Quizzes WHERE Id = 1;

-- ---------- Kısıt denemeleri (hata vermesi BEKLENİR) ----------
-- INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt) VALUES ('X', 'ogretmen@quizarena.test', 'h', 1, '2026-01-01'); -- UNIQUE Email
-- INSERT INTO Questions (QuizId, Text, OrderNo, TimeLimitSeconds, Points, CreatedAt) VALUES (1, 'X', 1, 999, 1000, '2026-01-01');    -- CHECK süre
-- INSERT INTO Quizzes (TeacherId, Title, DefaultTimeLimitSeconds, IsPublished, CreatedAt) VALUES (9999, 'X', 20, 0, '2026-01-01');   -- FK: öğretmen yok
