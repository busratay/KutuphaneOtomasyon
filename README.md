# 📚 Kütüphane Otomasyon Sistemi

Bu proje, **C# Windows Forms** ve **Entity Framework (DB-First)** kullanılarak geliştirilmiş, çok rollü kullanıcı yapısına sahip kapsamlı bir **Kütüphane Otomasyon Sistemidir**.  
Proje, kullanıcıların rollerine göre farklı yetkilere sahip paneller sunar: **Üye**, **Kütüphane Görevlisi**, **Yönetici**.

---

## 🛠️ Teknolojiler

- C# (.NET Framework)
- Windows Forms
- Entity Framework (DB-First)
- SQL Server
- GDI+ (arayüz özelleştirmeleri için)

---

## 👤 Kullanıcı Rolleri

Kullanıcılar `Kullanicilar`, `Roller` ve `KullaniciRolleri` tablosu üzerinden ilişkilendirilmiştir.  
Her kullanıcıya ait detay bilgileri ilgili detay tablolarında tutulur:

- `UyeDetay` → Üyelere ait bilgiler
- `KutuphaneGorevliDetay` → Görevlilere ait bilgiler

### Desteklenen Roller:
- 🧑‍🎓 Üye
- 📚 Kütüphane Görevlisi
- 🛡️ Yönetici

---

## 📂 Ana Özellikler

### 🧑‍🎓 Üye Paneli
- Kendi bilgilerini görüntüleme ve düzenleme
- Bakiye yükleme (Ceza varsa otomatik ödeme)
- Kitap ödünç talebi gönderme
- Aktif kitaplarını görüntüleme
- Emanet Geçmişini Görüntüleme

### 📚 Kütüphane Görevlisi Paneli
- Kendi bilgilerini görüntüleme ve düzenleme
- Üye listesi ve bilgilerini görüntüleme
- Kitap İşlemleri (ekleme / düzenleme / silme)
- Ödünç Talep İşlemleri (onayla/reddet) (online)
- Ödünç Verme (Yüz yüze)
- İade Alma İşlemleri (Yüz yüze)
- Geçmiş Emanetler (üyeye ve tarihe özel filtreleme ile)

### 🛡️ Yönetici Paneli
- Kendi bilgilerini görüntüleme ve düzenleme
- Kütüphaneci İşlemleri (Ekle/Sil)
- Üye Bilgileri (Statü değiştirme, aktif-pasif yapma, üye sil)
- Kitap Takip (Kitap şimdi nerede görme)
- Kİtap Talep (Talebi onaylanan,reddedilenleri görme ve raporları)
- Emanetler (Kitap şu an emanette mi teslim mi edildi görme)
- Geciken Kitaplar ve Ceza durumunu görme
- Raporlar (Grafikler ile gösteriliyor ayrıca kütüphanecinin yaptığı işlemler de var.)

---

## 🧩 Veritabanı Yapısı

Proje, **DB-First** yaklaşımı ile geliştirilmiş ve aşağıdaki tablolarla yapılandırılmıştır:

### 📌 Kullanıcı Tabloları ve Rolleri
- `Kullanicilar`: Tüm kullanıcıların temel bilgileri
- `Roller`: Rol tanımları
- `KullaniciRolleri`: Kullanıcı ile rol ilişkilendirmesi (çoktan çoğa)
- `UyeDetay`: Üyelere özel bilgiler
- `KutuphaneGorevliDetay`: Görevlilere özel bilgiler

### 📘 Kitap Yönetimi
- `Kitaplar`
- `Kategoriler`
- `Yazarlar`
- `Yayinevleri`

#### Ara Tablolar (Çoktan Çoğa İlişki):
- `KitapYazar`
- `KitapKategori`

### 🔄 Ödünç İşlemleri
- `OduncIslemleri`: Verilen kitaplar, iade durumu, tarih bilgisi
- `OduncTalepleri`: Üyelerden gelen taleplerin geçici olarak tutulduğu tablo

### 📊 Raporlama
- `Raporlar`: Okuma istatistikleri, gecikme bilgileri, en çok okunanlar vb.

## 🗃️ Veritabanı Dosyası

📁 `Database/KutuphaneOtomasyon.bak`

Bu dosyayı SQL Server Management Studio (SSMS) ile geri yükleyebilirsiniz:

> SSMS → Veritabanları → Sağ tık → Geri Yükle (Restore) → Cihaz → `.bak` dosyasını seç → Tamam

---
## 📄 Lisans

Bu proje açık kaynak değildir. Sadece staj projesi için paylaşılmıştır.

## ✍️ Geliştirici

**Büşra Tay**  

## 🚀 Kurulum Adımları

1. Depoyu klonlayın:
   ```bash
   git clone https://github.com/busratay/kutuphane-otomasyon.git
