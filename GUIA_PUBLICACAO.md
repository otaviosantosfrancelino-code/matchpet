# Guia de publicação do Match-Pet SP (Render + Neon + Brevo)

Este guia mostra como deixar o site **seguro**, **rápido** e com **e-mail funcionando**.
Nenhuma chave deve ficar no código: o repositório no GitHub é **público**.
Tudo abaixo é configurado no painel do Render, em **Environment** (variáveis de ambiente).

---

## 1. Banco de dados (Neon) — PRIORIDADE

A senha do banco esteve no `appsettings.json`, que está no GitHub. Troque-a:

1. Acesse **console.neon.tech** → seu projeto → **Roles** → `neondb_owner` → **Reset password**.
2. Copie a nova *connection string* (formato .NET).
3. No Render → seu serviço → **Environment** → **Add Environment Variable**:

| Nome | Valor |
|---|---|
| `ConnectionStrings__DefaultConnection` | `Server=...neon.tech;Database=neondb;Port=5432;User Id=neondb_owner;Password=NOVA_SENHA;Ssl Mode=Require;Trust Server Certificate=true;` |

4. No seu computador, guarde a mesma conexão fora do projeto:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "COLE_A_CONEXAO_AQUI"
```

5. Depois apague a senha antiga do `appsettings.json`.

> Atenção: **nunca** coloque chaves na pasta `wwwroot` (ex.: `wwwroot/.env`). Tudo nessa pasta pode ser baixado por qualquer pessoa que acesse o site.

---

## 2. E-mail (recuperação de senha e boas-vindas) — Brevo, grátis

O plano gratuito do Render **bloqueia envio por SMTP** (Gmail, Outlook). Por isso o site envia pelo **Brevo**,
que funciona pela internet (HTTPS) e tem plano grátis de 300 e-mails por dia.

1. Crie uma conta em **brevo.com** (grátis).
2. Menu **Senders, Domains & Dedicated IPs** → **Senders** → **Add a sender**: cadastre o e-mail que vai aparecer
   como remetente (ex.: `matchpet.tcc@gmail.com`) e confirme pelo link que chega nele.
3. Menu **SMTP & API** → aba **API Keys** → **Generate a new API key**. Copie a chave (começa com `xkeysib-`).
4. No Render → **Environment**, adicione:

| Nome | Valor |
|---|---|
| `Email__BrevoApiKey` | a chave `xkeysib-...` |
| `Email__RemetenteEmail` | o e-mail confirmado no passo 2 |
| `Email__RemetenteNome` | `Match-Pet SP` |

5. Salve. O Render reinicia o site sozinho. Teste em **Entrar → Esqueci minha senha**.

> Para testar no seu computador, use `dotnet user-secrets set "Email:BrevoApiKey" "xkeysib-..."` (e o remetente).
> Sem configuração, rodando no computador, o link de recuperação aparece na própria tela (modo de teste).

---

## 3. Leitura automática de documentos (IA) — opcional

| Nome | Valor |
|---|---|
| `ANTHROPIC_API_KEY` | chave criada em **console.anthropic.com** (paga por uso; centavos por documento) |

Sem a chave, os documentos ficam "Em Análise".

---

## 4. Site demorando para abrir no celular (Render grátis)

No plano gratuito, o Render **desliga o site depois de 15 minutos sem visitas**. A primeira visita depois disso
leva de 30 a 60 segundos para "acordar" — é isso que deixa o celular lento. Duas soluções:

**A) Grátis: manter o site acordado com um "ping"**
1. Crie uma conta em **cron-job.org** (ou **uptimerobot.com**).
2. Crie uma tarefa que acessa `https://SEU-SITE.onrender.com/health` **a cada 10 minutos**.
3. Pronto: o site não dorme mais. O endereço `/health` é leve e não usa o banco de dados.

> O plano grátis do Render dá 750 horas por mês — o suficiente para um site ligado o mês inteiro.

**B) Pago:** o plano *Starter* do Render (cerca de US$ 7/mês) nunca dorme.

O que já foi otimizado no código:
- Fotos carregam por endereço próprio, com cache no navegador (antes iam dentro da página).
- CSS e JavaScript são enviados comprimidos e com cache de longo prazo.
- O JavaScript do Bootstrap (não usado) foi removido e a fonte carrega em paralelo.
- A conexão com o banco tenta de novo sozinha quando o Neon "acorda".
- Celular que troca de rede (Wi‑Fi ↔ 4G) não perde a tela por até 5 minutos.

---

## 5. Contas de demonstração

O site cria cadastros de exemplo na primeira vez que abre (ONGs, pessoas, pets, itens, publicações, conversas).
Todas usam a senha **`Demo@123`** e e-mails terminados em `@demo.matchpet.com.br`.
Para não criar esses dados, adicione `SeedDemo` = `false` no Render.

## 6. Administradores e contas da banca

- As contas da banca (**administrador**, **adotante** e **ONG**, com e-mails `@matchpet.example`) são criadas quando o site roda no computador
  em modo de desenvolvimento. As senhas, aleatórias, ficam no arquivo local `CONTAS_BANCA.local.txt`, fora do GitHub.
- Para transformar a **sua própria conta** em administradora, adicione no Render a variável
  `Admin__Emails` = `seu-email@exemplo.com` (vários e-mails separados por `;`) e reinicie o serviço.
- O painel fica em **/admin**: denúncias, documentos em análise, aves aguardando aprovação, usuários e o resultado do questionário SUS.

## 7. Teste de usabilidade (SUS)

Compartilhe o link **https://matchpet.onrender.com/avaliar** com ONGs, adotantes e colegas.
O resultado (nota média de 0 a 100, médias por pergunta e comentários) aparece no painel do administrador, na aba "Avaliação SUS".
