📌 jekirdekCase Projesi - Geliştirme Süreci<br>
🚀 Proje Açıklaması<br><br>

jekirdekCase, PostgreSQL tabanlı çalışan, JWT ve Cookie Authentication desteği bulunan, ASP.NET Core MVC ile geliştirilmiş bir web uygulamasıdır.<br><br>

🔧 Yapılan Geliştirmeler<br><br>

1️⃣ Altyapı & Konfigürasyon<br>
✅ PostgreSQL veritabanı bağlandı ve CRMDbContext oluşturuldu.<br>
✅ appsettings.json içinde JWT yapılandırması eklendi.<br>
✅ FluentValidation ile model doğrulama yapısı kuruldu.<br>
✅ Dependency Injection (DI) ile servisler tanımlandı.<br><br>

2️⃣ Kimlik Doğrulama & Yetkilendirme<br>
✅ JWT Authentication eklendi ve token doğrulama işlemleri tamamlandı.<br>
✅ Cookie Authentication eklendi (Login/Logout işlemleri).<br>
✅ Kullanıcı yetkilendirmesi için Authorization middleware kullanıldı.<br><br>

3️⃣ Swagger & API Dökümantasyonu<br>
✅ Swagger UI eklendi ve SwaggerGen kullanılarak API dokümantasyonu oluşturuldu.<br>
✅ Token kullanımı için Swagger üzerinde Bearer Authentication ayarlandı.<br><br>

4️⃣ Razor Pages Geliştirmeleri<br>
✅ Account/Login ve Account/Logout sayfaları oluşturuldu.<br>
✅ Yetkisiz kullanıcılar /customers/customers sayfasına eriştiğinde otomatik login sayfasına yönlendirme yapıldı.<br>
✅ Login olan kullanıcılar için otomatik "Customers" sayfasına yönlendirme eklendi.<br><br>

5️⃣ Routing & Middleware Güncellemeleri<br>
✅ Swagger'ın sadece Development ortamında açılması sağlandı.<br>
✅ Varsayılan URL (/) açıldığında:<br><br>

Eğer kullanıcı login değilse, /Account/Login sayfasına yönlendiriliyor.<br>
Eğer kullanıcı login olmuşsa, /customers/customers sayfasına yönlendiriliyor.<br>
✅ Middleware ile özel yönlendirme mekanizması yazıldı.<br><br>



Giriş bilgileri<br>
Kullanıcı adı : admin<br>
şifre : admin123<br>
