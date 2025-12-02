# Cone
チケットベースで学生がTA/SAに質問できる機能を備えた講義支援システムです。 チケットごとの担当変更機能や学生班ごとの進捗管理機能を備えており、講義運営を効率化できます。
東京工科大学 先進情報専門演習・基盤I/II[IP・3] で実際に使用されています。

## 準備
- `global.json`に記載の.NET SDKをインストールしてください。
- `appsettings.Development.json`を作成し、`appsettings.json.example`を参考に設定してください。

## マイグレーション
### 作成
```bash
dotnet ef migrations add hogehoge --project Cone.csproj --context ConeDbContext -- --environment Development
```

## SQLの確認
```bash
dotnet ef migrations script --project Cone.csproj --context ConeDbContext -- --environment Development
```

### 適用
```bash
dotnet ef database update --project Cone.csproj --context ConeDbContext -- --environment Development
```

## 貢献
プルリクエストを歓迎します。  バグ報告や機能提案はIssueにてお願いします。

## 名前の由来
https://x.com/icol_official