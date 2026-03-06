
# Plaid Integration Flow (React Native + .NET Backend)

## Amaç
Kullanıcının banka hesaplarına erişmek ve hesap ekstrelerini çekebilmek için Plaid üzerinden kullanıcıdan yetki almak.

Bu doküman **kullanıcı "Hesap Ekle" butonuna bastığı andan itibaren** gerçekleşecek tüm akışı açıklar.

---

# 1. Kullanıcı Hesap Ekle Sürecini Başlatır

## UI
Kullanıcı **"Add Bank Account" / "Hesap Ekle"** butonuna basar. open bankin secmesinden itibaen ilerlemek isterse asagidaki adimlar uygulanir

react native taraifnda Plaid Link SDK kullanilsin, onerildigi gibi best practicelerle entegrasyon saglansin

UI şu isteği backend'e gönderir:

```
POST /api/plaid/create-link-token
```


# 2. Backend Link Token Oluşturur

Backend Plaid'e aşağıdaki endpoint ile istek atar.

```
POST /link/token/create
```

Plaid API base url:

```
https://sandbox.plaid.com/link/token/create
```

### Request Body

```
{
  "client_id": "PLAID_CLIENT_ID", app settingsden okunur
  "secret": "PLAID_SECRET", app settingsden okunur
  "client_name": "Your App Name", constant olustur ordan al
  "country_codes": ["US","GB","CA"], UK hard coded olsun
  "language": "en", uygulamanin dili
  "user": {
    "client_user_id": "internal-user-id" user.id
  },
  "products": ["transactions"],
}
```

### Response

```
{
  "link_token": "link-sandbox-xxxx",
  "expiration": "2026-03-07T00:00:00Z" 
}
```

### Backend Yapması Gerekenler

Backend sadece **link_token** değerini UI'a döner.

### Response to UI

```
{
  "linkToken": "link-sandbox-xxxx"
}
```

---

# 3. UI Plaid Link'i Açar

React Native tarafında Plaid Link açılır. bu sdk araciligiyle yapilir

Kullanıcı:

1. Bankasını seçer
2. Banka login bilgilerini girer
3. Hesaplara erişim izni verir

Bu işlemler Plaid'in UI'ında gerçekleşir.

Başarılı olunca UI'a şu bilgiler döner:

```
public_token
metadata
```

### Örnek

```
{
  "public_token": "public-sandbox-xxxx",
  "metadata": {
    "institution": {
      "name": "Chase"
    },
    "accounts": [
      {
        "id": "account_id",
        "name": "Checking"
      }
    ]
  }
}
```

---

# 4. UI Public Token'ı Backend'e Gönderir

UI şu endpointi çağırır:

```
POST /api/plaid/exchange-token
```

### Request

```
{
  "publicToken": "public-sandbox-xxxx"
}
```

---

# 5. Backend Public Token'ı Access Token'a Çevirir

Backend Plaid'e şu isteği gönderir.

```
POST /item/public_token/exchange
```

### Request


```
{
  "client_id": "PLAID_CLIENT_ID",
  "secret": "PLAID_SECRET",
  "public_token": "public-sandbox-xxxx"
}
```

### Response
UserPlaidToken adinda bir entity olustur bu bilgileri orda sakla, bir kullanicinin birden fazla tokeni olamaz ya sil tekrar ekle ya da guncelleme yap. Secret en guvenli sekilde sifrelenerek saklanmali, access token frontend'e asla gonderilmemeli
```
{
  "access_token": "access-sandbox-xxxx",
  "item_id": "item-xxxx"
}
```

---

---


# 11. Özet Akış

1. Kullanıcı **Add Bank Account** butonuna basar
2. UI → backend **link token** ister
3. Backend → Plaid `/link/token/create`
4. UI Plaid Link açar
5. Kullanıcı bankaya giriş yapar
6. UI **public_token** alır
7. UI → backend gönderir
8. Backend → `/item/public_token/exchange`
9. **access_token alınır**
10. access_token DB'de saklanır

---

# 12. Güvenlik Notları

- `access_token` **asla frontend'e gönderilmez**
- sadece backend'de saklanır
- **encrypted olarak saklanmalıdır**

---


# 14. Kurallar
- Backend tum plaid iletisimini loglar, ayni stripeda oldugu gibi
