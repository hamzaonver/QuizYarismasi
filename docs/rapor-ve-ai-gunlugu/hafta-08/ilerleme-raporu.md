# Haftalık İlerleme Raporu — Hafta 8

> Şablon 9 · Yazdırılabilir sürüm: `Hafta-08-Ilerleme-Raporu.docx`

| Öğrenci | Numara | Proje | Tarih |
|---|---|---|---|
| Hamza Önver | 251161043 | P60 — Sınıf İçi Oyunlaştırılmış Quiz Yarışması (QuizArena) | 12.11.2026 |

**Hafta No:** 8 · VİZE: Ara Sürüm, Entegrasyon ve Sözlü Hazırlık

## Bu hafta tamamlanan işler

- Modülleri uçtan uca entegre ettim: kullanıcı (H5) + veritabanı (H4) + quiz (H6) + canlı oturum (H7) tek akışta çalışıyor.
- Otomatik entegrasyon testi yazıldı (scripts/entegrasyon-testi.bat): 38 kontrol, geçici veritabanı kullanıyor, gerçek veriye dokunmuyor.
- 4 bilinen hata düzeltildi: açık oturumlu quizin yayından kaldırılması, aynı anda aynı takma adla katılımda hata sayfası, ana sayfadaki eski 'Yakında' yazısı, telefonda görünmeyen menü ikonu.
- Canlı demo linki: scripts/demo-link.bat siteyi açıp internetten erişilebilen geçici bir link (QR kodlu) veriyor; linke giren hoca 'Nasıl denenir?' rehberiyle kayıt olup deneyebiliyor.
- Ara sürüm v0.8.0: Git tag + GitHub Release, CHANGELOG.md, sürüm numarası sitenin altında.
- Vize demo senaryosu (docs/demo-senaryosu.md) ve sözlü sınav cevapları (docs/haftalar/hafta-08.md) hazırlandı; mimari doküman güncellendi.

## Karşılaşılan sorunlar

- Oturum açıkken quiz yayından kaldırılabiliyordu; öğrenciler taslak bir quizle bekleme salonunda kalıyordu (Quiz ve Oturum modülleri arasında entegrasyon hatası).
- İki öğrenci aynı anda aynı takma adla katılırsa ikincisi hata sayfası görüyordu.
- Proje kendi bilgisayarımda derleniyor ama site açılmıyordu (Windows Akıllı Uygulama Denetimi yeni derlenen DLL'i engelliyordu).
- Otomatik test gerçek veritabanında çalışırsa demo verilerimi kirletebilirdi.

## Uygulanan çözümler

- QuizService yayından kaldırmadan önce açık oturum olup olmadığına bakıyor (QuizRepository.HasOpenSessionAsync).
- SessionService.JoinAsync veritabanı hatasını yakalayıp 'Bu takma ad az önce alındı' mesajı gösteriyor.
- Test betiğine hata kaydı ekleyip sebebi buldum; Windows Güvenliği'nden Akıllı Uygulama Denetimi'ni kapattım.
- Entegrasyon testi ortam değişkeniyle (ConnectionStrings__Default) geçici bir veritabanı açıyor ve sonunda siliyor.

## Yapay zekâ kullanımı özeti (günlüğe atıf)

Entegrasyon hatalarını ayıklamak, test betiğini yazmak, sürümleme ve dokümanlar için kullandım; her değişikliğin açıklaması hafta-08.md'de (Bu haftanın AI günlüğü, 1–6. satırlar). → [ai-kullanim-gunlugu.md](ai-kullanim-gunlugu.md)

## Sonraki hafta planı

Hafta 9: SignalR (WebSocket) ile gerçek zamanlı iletişim; öğrencinin cevap vermesi, olay yayını, bağlantı koparsa yeniden bağlanma.

## Ekler

- Ara sürüm: https://github.com/hamzaonver/QuizYarismasi/releases/tag/v0.8.0
- Entegrasyon testi sonucu: TÜM KONTROLLER BAŞARILI (38 kontrol)
- Notlar: docs/haftalar/hafta-08.md · Demo: docs/demo-senaryosu.md
