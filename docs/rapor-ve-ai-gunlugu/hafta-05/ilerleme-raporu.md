# Haftalık İlerleme Raporu — Hafta 5

> Şablon 9 · Yazdırılabilir sürüm: `Hafta-05-Ilerleme-Raporu.docx`

| Öğrenci | Numara | Proje | Tarih |
|---|---|---|---|
| Hamza Önver | 251161043 | P60 — Sınıf İçi Oyunlaştırılmış Quiz Yarışması (QuizArena) | 15.10.2026 |

**Hafta No:** 5 · Kullanıcı Yönetimi ve Kimlik Doğrulama

## Bu hafta tamamlanan işler

- Kayıt ve giriş ekranları (AccountController + AuthService).
- Parolalar BCrypt ile hash'leniyor, düz metin saklanmıyor.
- Parola kuralı: en az 8 karakter, büyük harf, küçük harf ve rakam.
- Çerez tabanlı oturum (HttpOnly, 2 saat) ve roller: Teacher / Student.
- 5 hatalı girişte hesap 5 dakika kilitleniyor; tüm formlarda CSRF koruması var.
- Kısa güvenlik notu yazıldı (docs/guvenlik-notu.md).

## Karşılaşılan sorunlar

- Herkesin kendini öğretmen olarak kaydedebilmesi güvenlik açığı olurdu.
- Öğrencinin, adresi elle yazarak öğretmen sayfalarına girmesi engellenmeliydi.

## Uygulanan çözümler

- Öğretmen kaydına davet kodu şartı koydum.
- Controller'lara [Authorize(Roles = ...)] ekledim; sahiplik kontrolünü iş katmanında yaptım.

## Yapay zekâ kullanımı özeti (günlüğe atıf)

Kimlik doğrulama yapısı, parola hash'leme ve güvenlik kontrolleri için kullandım (Bu haftanın AI günlüğü, 1–2. satırlar). → [ai-kullanim-gunlugu.md](ai-kullanim-gunlugu.md)

## Sonraki hafta planı

Quiz oluşturma modülü: quiz, soru, şık, doğru cevap, süre.

## Ekler

- Güvenlik notu: docs/guvenlik-notu.md
- Demo hesaplar: ogretmen@quizarena.test, ogrenci1@quizarena.test
