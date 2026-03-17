# Dashboard veri yükleme davranışı — Araştırma

Kısa özet

Mevcut implementasyonda Dashboard verisi store'da bir kez yüklendikten sonra manuel pull-to-refresh yapılana veya uygulama/yeniden başlatılana kadar tekrar API'ye gönderilmiyor. Bu davranış, ekranın mount anında yalnızca "data yoksa" koşulu ile `loadHomepage()` thunk'ını çağırmasından kaynaklanır.

İlgili dosyalar

- `src/screens/dashboard/DashboardScreen.tsx` — UI, `useEffect` içinde `loadHomepage()` çağrısı; pull-to-refresh için `refreshHomepage()` kullanımı.
- `src/store/homepageStore.ts` — Redux Toolkit slice; `loadHomepage` ve `refreshHomepage` async thunk'ları; `isLoading`, `isRefreshing`, `data`, `error` alanları.
- `src/services/homepageService.ts` — `homepageService.getHomepage()` HTTP çağrısı wrapper'ı.
- `src/services/apiClient.ts` — `apiCall()` helper ve axios client; hataları çevirir.
- `src/store/index.ts` — store birleşimi (homepage reducer burada kayıtlı).

Akış (UI → store → API)

1. `DashboardScreen` mount olduğunda `useEffect` çalışır:
   - Koşul: `if (!data && !isLoading && !error) dispatch(loadHomepage());`
   - Yani: store'da `data` varsa `loadHomepage` tetiklenmez.
2. `loadHomepage` thunk çalışır:
   - `apiCall(() => homepageService.getHomepage())` çağrılır ve `apiClient` üzerinden GET yapılır.
   - Başarılıysa `loadHomepage.fulfilled` ile `state.data` setlenir.
3. Kullanıcı pull-to-refresh yaparsa `refreshHomepage()` çağrılır ve `isRefreshing` kontrolüyle istek yapılır.

Neden sadece bir kere çağrı yapılıyor (teknik olarak)

- `DashboardScreen`'deki `useEffect` koşulu sadece `data` yoksa çağrı yapacak şekilde yazılmıştır.
- Store'da `data` tutulan bir state olduğu için, ekran tekrar mount olsa bile `data` mevcutsa tekrar çağrı yapılmaz.
- Otomatik focus/fresh veya TTL tabanlı yeniden çekme mekanizması yoktur.

Store yapısı ve flaglerin anlamı

- `data: HomepageResponse | undefined` — API cevabı.
- `isLoading: boolean` — ilk yükleme sırasında true.
- `isRefreshing: boolean` — pull-to-refresh sırasında true.
- `error: string | undefined` — hata mesajı; hata varsa ve `data` yoksa hata ekranı gösterilir.

Kullanıcı senaryoları

- Uygulama ilk açıldığında Dashboard yüklenecek.
- Sekmeler arası geçiş veya ekran tekrar açılması durumunda store'da `data` bulunduğu sürece yeniden yükleme yapılmaz.
- Kullanıcı elle çekerse `refreshHomepage()` çağrılır ve veri güncellenir.

Öneriler ve uygulanabilir yaklaşımlar

1) Ekran focus olduğunda yeniden yükleme

   - `@react-navigation/native`'dan `useFocusEffect` veya `useIsFocused` kullanın.
   - Örnek:

```ts
import {useFocusEffect} from '@react-navigation/native';
useFocusEffect(
  React.useCallback(() => {
    if (!data) dispatch(loadHomepage());
    return () => {};
  }, [data])
);
```

2) TTL (time-to-live) ile cache invalidation

   - `homepageStore`'a `lastFetched?: number` alanı ekleyin. `fulfilled` olduğunda `lastFetched = Date.now()` set edin.
   - `DashboardScreen`'de `if (!data || Date.now() - lastFetched > TTL) dispatch(loadHomepage())` şeklinde kontrol yapın.
   - Kısa örnek slice güncellemesi:

```ts
// state: { data?: HomepageResponse; lastFetched?: number; ... }
// on fulfilled:
state.data = action.payload;
state.lastFetched = Date.now();
```

3) AppState veya foreground listener ile yenileme

   - `AppState` listener ile uygulama foreground'a gelince TTL kontrolü yaparak gerektiğinde `refreshHomepage()` çağırın.

4) RTK Query kullanımı

   - RTK Query ile cache, invalidation, polling ve otomatik refetch-on-focus/refetch-on-reconnect gibi özellikleri kolayca kullanabilirsiniz.

5) Push veya sunucu tarafı invalidation

   - Sunucudan gönderilen push bildirimleriyle verinin stale olduğu bildirilirse `refreshHomepage()` tetiklenebilir.

Debug checklist

- `DashboardScreen` içindeki useEffect koşulunu kontrol edin: `if (!data && !isLoading && !error)`
- `refreshHomepage()` gerçekten ağ çağrısı yapıyor mu? (network logs / console)
- Store'da `data` uzun süre saklanıyorsa ve TTL yoksa bu beklenen davranıştır.
- `apiClient` tarafında özel bir cache mekanizması yok (axios default olarak cache kullanmaz).

Kısa kod örnekleri (özet)

- Focus ile refetch:

```ts
useFocusEffect(
  React.useCallback(() => {
    const shouldLoad = !data;
    if (shouldLoad) dispatch(loadHomepage());
  }, [data])
);
```

- TTL kontrolü örneği (DashboardScreen useEffect içinde):

```ts
const TTL = 1000 * 60 * 5; // 5 dakika
useEffect(() => {
  const shouldLoad = !data || !lastFetched || (Date.now() - lastFetched) > TTL;
  if (shouldLoad && !isLoading && !error) dispatch(loadHomepage());
}, [data, lastFetched, isLoading, error]);
```

Sonuç

Mevcut davranış, store'da veri tutulması ve `useEffect` koşulunun yalnızca "data yoksa" kontrolü nedeniyle ortaya çıkıyor. Otomatik yenileme veya zaman bazlı invalidation isteniyorsa `useFocusEffect`, TTL, AppState listener veya RTK Query gibi yaklaşımlardan biri uygulanmalıdır.

Dosya oluşturuldu: `docs/DashboardCacheBehavior.md`

