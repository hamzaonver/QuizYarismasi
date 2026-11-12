# Haftalık İlerleme Raporu — Hafta 6

> Şablon 9 · Yazdırılabilir sürüm: `Hafta-06-Ilerleme-Raporu.docx`

| Öğrenci | Numara | Proje | Tarih |
|---|---|---|---|
| Hamza Önver | 251161043 | P60 — Sınıf İçi Oyunlaştırılmış Quiz Yarışması (QuizArena) | 15.10.2026 |

**Hafta No:** 6 · Çekirdek Modül A: Quiz Oluşturma

## Bu hafta tamamlanan işler

- Quiz → Soru → Şık yapısı; her soruda 2–6 şık ve tek doğru cevap.
- Quiz için varsayılan süre, her soru için ayrı süre (5–300 sn) ve puan.
- Quiz oluştur, düzenle, sil, yayınla/taslağa al, kopyala.
- Soru ekle, düzenle, sil, yukarı/aşağı taşı.
- İş kuralları QuizValidator ve QuizRules içinde; sahiplik kontrolü QuizService'te.

## Karşılaşılan sorunlar

- Doğru cevabın öğrenciye gösterilip gösterilmeyeceği belirsizdi.
- Oturumu açılmış bir quizin soruları değişirse sonuçlar bozulabilirdi.

## Uygulanan çözümler

- Doğru cevabı sadece öğretmen ekranında gösterme kararı aldım (öğrenciye hiç gönderilmiyor).
- Oturumu olan quizin sorularını kilitledim; sadece kopyalayıp yeni quiz olarak düzenlenebiliyor.

## Yapay zekâ kullanımı özeti (günlüğe atıf)

Modül kodu, doğrulama kuralları ve kavramları (DTO, entity) anlamak için kullandım (Bu haftanın AI günlüğü, 1–3. satırlar). → [ai-kullanim-gunlugu.md](ai-kullanim-gunlugu.md)

## Sonraki hafta planı

Canlı oturum: katılım kodu, öğrencinin oturuma katılması, oturum durumu.

## Ekler

- Commit: https://github.com/hamzaonver/QuizYarismasi/commit/2341a3a
- Notlar: docs/haftalar/hafta-06.md
