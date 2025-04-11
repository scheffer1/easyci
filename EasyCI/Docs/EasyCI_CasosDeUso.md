# Diagrama de Casos de Uso - EasyCI

Este documento descreve os casos de uso do sistema EasyCI, uma aplicação para gerenciamento de integração contínua com repositórios Git e containers Docker.

## Atores

### Usuário
Representa o usuário final do sistema que interage com a interface gráfica para gerenciar repositórios, containers e projetos de CI.

### Sistema
Representa os processos automatizados que executam tarefas em segundo plano, como clonagem de repositórios Git.

## Casos de Uso

### Gerenciamento de Repositórios Git

1. **Gerenciar Repositórios Git**
   - Descrição: Permite ao usuário gerenciar os repositórios Git cadastrados no sistema.
   - Ator principal: Usuário
   - Inclui:
     - Cadastrar Repositório Git
     - Editar Repositório Git
     - Excluir Repositório Git

2. **Cadastrar Repositório Git**
   - Descrição: Permite ao usuário cadastrar um novo repositório Git, informando URL, branch, chave SSH, etc.
   - Ator principal: Usuário

3. **Editar Repositório Git**
   - Descrição: Permite ao usuário editar as informações de um repositório Git existente.
   - Ator principal: Usuário

4. **Excluir Repositório Git**
   - Descrição: Permite ao usuário excluir um repositório Git do sistema.
   - Ator principal: Usuário

### Gerenciamento de Containers Docker

5. **Gerenciar Containers Docker**
   - Descrição: Permite ao usuário gerenciar os containers Docker cadastrados no sistema.
   - Ator principal: Usuário
   - Inclui (não mostrado no diagrama):
     - Cadastrar Container Docker
     - Editar Container Docker
     - Excluir Container Docker

### Gerenciamento de Projetos CI

6. **Gerenciar Projetos CI**
   - Descrição: Permite ao usuário gerenciar os projetos de integração contínua.
   - Ator principal: Usuário
   - Inclui:
     - Criar Projeto CI
     - Editar Projeto CI (não mostrado no diagrama)
     - Excluir Projeto CI (não mostrado no diagrama)

7. **Criar Projeto CI**
   - Descrição: Permite ao usuário criar um novo projeto CI, associando um repositório Git a um container Docker.
   - Ator principal: Usuário

### Execução e Monitoramento de Builds

8. **Executar Build**
   - Descrição: Permite ao usuário iniciar manualmente o processo de build de um projeto CI.
   - Ator principal: Usuário
   - Inclui:
     - Clonar Repositório Git via SSH

9. **Clonar Repositório Git via SSH**
   - Descrição: Processo de clonagem de um repositório Git usando autenticação SSH.
   - Ator principal: Sistema

10. **Monitorar Builds**
    - Descrição: Permite ao usuário acompanhar o status e progresso dos builds em execução.
    - Ator principal: Usuário
    - Inclui:
      - Visualizar Detalhes do Build

11. **Visualizar Detalhes do Build**
    - Descrição: Permite ao usuário visualizar informações detalhadas sobre um build específico, incluindo logs e status.
    - Ator principal: Usuário

## Como Usar o Diagrama

O diagrama de casos de uso está disponível no arquivo `EasyCI_CasosDeUso.drawio` e pode ser aberto e editado usando a ferramenta [draw.io](https://app.diagrams.net/).

Para abrir o diagrama:
1. Acesse https://app.diagrams.net/
2. Clique em "Abrir Arquivo Existente"
3. Selecione o arquivo `EasyCI_CasosDeUso.drawio`

Alternativamente, você pode usar a extensão Draw.io Integration para Visual Studio Code para abrir e editar o diagrama diretamente no seu ambiente de desenvolvimento.
