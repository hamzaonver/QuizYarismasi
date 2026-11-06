# Haftalık İlerleme Raporu — Hafta 7

> Şablon 9 · Yazdırılabilir sürüm: `Hafta-07-Ilerleme-Raporu.docx`

| Öğrenci | Numara | Proje | Tarih |
|---|---|---|---|
|  |  | P60 — Sınıf İçi Oyunlaştırılmış Quiz Yarışması (QuizArena) | 05.11.2026 |

**Hafta No:** 7 · Çekirdek Modül B: Canlı Oturum ve Katılım + Entegrasyon

## Bu hafta tamamlanan işler

- Öğretmen yayındaki quizden oturum açıyor; 6 karakterlik benzersiz katılım kodu üretiliyor.
- Öğrenci kod ve takma adla katılıyor, bekleme salonuna düşüyor.
- Oturum durumları: Bekleme salonu → Devam ediyor → Bitti.
- Öğretmen ekranı: katılımcı listesi, Başlat / Sonraki soru / Bitir / İptal.
- Ekranlar 3 saniyede bir durumu kontrol ediyor; öğrenci ekranı soruya kendiliğinden geçiyor.
- Hafta 5 (roller) ve Hafta 6 (yayın durumu, soru kilidi) ile entegre edildi; yeni migration gerekmedi.

## Karşılaşılan sorunlar

- Öğrenci sayfa kaynağına bakarak doğru cevabı görebilirdi.
- Başlamış bir oturuma sonradan giren öğrenci soruları kaçırırdı.
- Aynı takma adla iki öğrenci girerse lider tablosunda karışıklık olurdu.

## Uygulanan çözümler

- Öğrenciye giden veride (LiveQuestionDto) doğru cevap alanını hiç koymadım.
- Sadece bekleme salonundaki oturuma katılmaya izin verdim.
- Aynı oturumda takma adı benzersiz yaptım (büyük/küçük harf fark etmeden).

## Yapay zekâ kullanımı özeti (günlüğe atıf)

Oturum servisinin ve ekranların kodu için kullandım; doğru cevabın öğrenciye gitmemesi kuralını ben istedim (Bu haftanın AI günlüğü, 1–2. satırlar). → [ai-kullanim-gunlugu.md](ai-kullanim-gunlugu.md)

## Sonraki hafta planı

Hafta 8 (vize): modülleri uçtan uca birleştirmek, bilinen hataları gidermek, ara sürümü Git tag ile etiketlemek (v0.8.0), demo senaryosunu prova etmek.

## Ekler

- Commit: https://github.com/hamzaonver/QuizYarismasi/commit/d029321
- Notlar ve demo adımları: docs/haftalar/hafta-07.md
