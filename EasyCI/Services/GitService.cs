using EasyCI.Models;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EasyCI.Services
{
    /// <summary>
    /// Serviço para operações Git
    /// </summary>
    public class GitService
    {
        /// <summary>
        /// Clona um repositório Git via SSH
        /// </summary>
        /// <param name="repository">Repositório Git a ser clonado</param>
        /// <param name="localPath">Caminho local onde o repositório será clonado</param>
        /// <returns>Resultado da operação de clone</returns>
        public async Task<(bool Success, string Message)> CloneRepositoryAsync(GitRepository repository, string localPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Verificar se o diretório já existe
                    if (Directory.Exists(localPath))
                    {
                        // Se já existe um repositório Git, apenas atualizar
                        if (Directory.Exists(Path.Combine(localPath, ".git")))
                        {
                            return UpdateRepository(repository, localPath);
                        }
                        else
                        {
                            // Se o diretório existe mas não é um repositório Git, remover e clonar
                            Directory.Delete(localPath, true);
                        }
                    }

                    // Criar o diretório pai se não existir
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));

                    // Configurar as opções de clone
                    var cloneOptions = new CloneOptions
                    {
                        BranchName = repository.Branch,
                        Checkout = true,
                        RecurseSubmodules = true
                    };

                    // Configurar as credenciais SSH se o caminho da chave for fornecido
                    if (!string.IsNullOrEmpty(repository.SshKeyPath) && File.Exists(repository.SshKeyPath))
                    {
                       // cloneOptions. = CreateCredentialsHandler(repository);
                    }

                    // Clonar o repositório
                    Repository.Clone(repository.Url, localPath, cloneOptions);

                    return (true, $"Repositório clonado com sucesso em {localPath}");
                }
                catch (Exception ex)
                {
                    return (false, $"Erro ao clonar repositório: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Atualiza um repositório Git existente
        /// </summary>
        /// <param name="repository">Repositório Git a ser atualizado</param>
        /// <param name="localPath">Caminho local do repositório</param>
        /// <returns>Resultado da operação de atualização</returns>
        private (bool Success, string Message) UpdateRepository(GitRepository repository, string localPath)
        {
            try
            {
                using (var repo = new Repository(localPath))
                {
                    // Configurar as credenciais SSH se o caminho da chave for fornecido
                    FetchOptions fetchOptions = new FetchOptions();
                    if (!string.IsNullOrEmpty(repository.SshKeyPath) && File.Exists(repository.SshKeyPath))
                    {
                        fetchOptions.CredentialsProvider = CreateCredentialsHandler(repository);
                    }

                    // Obter a referência remota (origin)
                    var remote = repo.Network.Remotes["origin"];
                    var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

                    // Fetch das alterações
                    Commands.Fetch(repo, remote.Name, refSpecs, fetchOptions, "Fetch de atualizações");

                    // Checkout da branch especificada
                    var branch = repo.Branches[$"origin/{repository.Branch}"];
                    if (branch != null)
                    {
                        // Resetar para a branch remota
                        repo.Reset(ResetMode.Hard, branch.Tip);
                    }
                    else
                    {
                        return (false, $"Branch '{repository.Branch}' não encontrada no repositório remoto");
                    }
                }

                return (true, $"Repositório atualizado com sucesso em {localPath}");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao atualizar repositório: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria um handler de credenciais para autenticação SSH
        /// </summary>
        /// <param name="repository">Repositório Git com as informações de autenticação</param>
        /// <returns>Handler de credenciais</returns>
        private CredentialsHandler CreateCredentialsHandler(GitRepository repository)
        {
            return (url, usernameFromUrl, types) =>
            {
                // Usar o nome de usuário fornecido ou o do URL
                string username = !string.IsNullOrEmpty(repository.Username) ? repository.Username : usernameFromUrl;

                // // Criar credenciais SSH
                // return new SshUserKeyCredentials
                // {
                //     Username = username,
                //     Passphrase = string.Empty, // Assumindo que a chave não tem senha
                //     PublicKey = repository.SshKeyPath + ".pub", // Caminho da chave pública (assumindo convenção de nome)
                //     PrivateKey = repository.SshKeyPath // Caminho da chave privada
                // };
                return new DefaultCredentials();
            };
        }
    }
}
