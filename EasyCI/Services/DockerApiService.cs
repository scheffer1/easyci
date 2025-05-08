using Docker.DotNet;
using Docker.DotNet.Models;
using EasyCI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EasyCI.Services
{
    public class DockerApiService
    {
        private readonly Dictionary<int, DockerClient> _clientCache = new();

        private DockerClient GetOrCreateClient(DockerContainer container)
        {
            if (_clientCache.TryGetValue(container.Id, out var cachedClient))
            {
                return cachedClient;
            }

            var client = CreateClient(container);
            _clientCache[container.Id] = client;
            return client;
        }

        private DockerClient CreateClient(DockerContainer container)
        {
            var endpoint = new Uri($"tcp://{container.Host}:{container.Port}");

            if (container.UseTLS && !string.IsNullOrEmpty(container.CertificatePath))
            {
                var credentials = new DockerClientConfiguration(
                    endpoint,
                    new AnonymousCredentials()
                );
                return credentials.CreateClient();
            }
            else
            {
                var credentials = new DockerClientConfiguration(
                    endpoint,
                    new AnonymousCredentials()
                );
                return credentials.CreateClient();
            }
        }

        public async Task<(bool Success, string Message)> TestConnectionAsync(DockerContainer container)
        {
            try
            {
                var client = GetOrCreateClient(container);
                var info = await client.System.GetSystemInfoAsync();
                return (true, $"Conexão bem-sucedida. Docker versão: {info.ServerVersion}");
            }
            catch (Exception ex)
            {
                return (false, $"Falha na conexão: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CreateDirectoryAsync(DockerContainer container, string path)
        {
            try
            {
                var client = GetOrCreateClient(container);

                var createResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = "alpine",
                    Cmd = new[] { "mkdir", "-p", path },
                    HostConfig = new HostConfig
                    {
                        Binds = new[] { $"{Path.GetDirectoryName(path)}:{Path.GetDirectoryName(path)}" }
                    }
                });

                await client.Containers.StartContainerAsync(createResponse.ID, null);
                var waitResponse = await client.Containers.WaitContainerAsync(createResponse.ID);
                await client.Containers.RemoveContainerAsync(createResponse.ID, new ContainerRemoveParameters());

                if (waitResponse.StatusCode == 0)
                {
                    return (true, $"Diretório criado com sucesso: {path}");
                }
                else
                {
                    return (false, $"Falha ao criar diretório. Código de status: {waitResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao criar diretório: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ExecuteDockerComposeAsync(
            DockerContainer container,
            string workingDirectory,
            string composeFilePath)
        {
            try
            {
                var client = GetOrCreateClient(container);

                var createResponse = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = "docker/compose",
                    WorkingDir = workingDirectory,
                    Cmd = new[] { "up", "-d", "-f", composeFilePath },
                    HostConfig = new HostConfig
                    {
                        Binds = new[]
                        {
                            $"{workingDirectory}:{workingDirectory}",
                            "/var/run/docker.sock:/var/run/docker.sock"
                        }
                    }
                });

                await client.Containers.StartContainerAsync(createResponse.ID, null);
                var waitResponse = await client.Containers.WaitContainerAsync(createResponse.ID);

                var logs = await client.Containers.GetContainerLogsAsync(
                    createResponse.ID,
                    new ContainerLogsParameters { ShowStdout = true, ShowStderr = true });

                await client.Containers.RemoveContainerAsync(createResponse.ID, new ContainerRemoveParameters());

                if (waitResponse.StatusCode == 0)
                {
                    return (true, "Docker Compose executado com sucesso");
                }
                else
                {
                    return (false, $"Falha ao executar Docker Compose. Código de status: {waitResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao executar Docker Compose: {ex.Message}");
            }
        }
    }
}
