# .NET Microservices - Full Course

A follow-along learning with [Les Jackson's .NET Microservices - Full Course video on YouTube](https://www.youtube.com/watch?v=DgVjEo3OGBI).

There is no Part 1 as it is introduction and theory only. You need to watch the video to know what this full course is about.

## Part 2 - Building The First Service

Some differences compared to the full course video:
1. Full course uses **.NET 5**.
   - I am using **.NET 8**.
3. Full course installs `AutoMapper.Extensions.Microsoft.DependencyInjection`.
   - At the time of this making, this package is deprecated and is suggested to install `AutoMapper` instead.

## Part 3 - Docker and Kubernetes

### Docker Hub and Docker Desktop

1. Sign up an account at [Docker Hub](https://hub.docker.com/).
2. Download and install [Docker Desktop](https://docs.docker.com/get-started/get-docker/). Once the installation finished, it will prompt for a restart.
3. Update Windows Subsystem for Linux (WSL) to latest version by opening a Command Prompt and run `wsl.exe --update`.
4. Enable **Kubernetes** on **Docker Desktop** by going to **Settings** `>` **Kubernetes**.
   - Choose the option to create a single-node cluster.
   - Click **Apply & restart**.
  
### Docker command cheat sheet

> [!NOTE]
> Before executing a Docker command, make sure your command prompt directory has been changed to where the **Dockerfile** is located.

* To check if Docker is running: `docker --version`.
* To build the Docker image: `docker build -t <your_docker_hub_id>/platformservice .`.
  - Make sure to include the period at the end as it is part of the command.
* To run the image: `docker run -p 8080:80 -d <your_docker_hub_id>/platformservice`. 
  - If `docker run -p 8080:80 -d <your_docker_hub_id>/platformservice` command is executed again, it will create a new container.
    - It **does not start up** the existing container.
  - For [.NET 8](https://github.com/dotnet/dotnet-docker/issues/5194), try `docker run -it --rm -p 8080:8080 --name platformservicecontainer <your_docker_hub_id>/platformservice`. 
* To list running containers: `docker ps`.
* To stop a running container: `docker stop <container_id>` (e.g. `docker stop 3add933fd2f3`).
* To start an existing container: `docker start <container_id>` (e.g. `docker start 3add933fd2f3`).
* To push the image up to **Docker Hub**: `docker push <your_docker_hub_id>/platformservice`.

### Run Docker image for .NET 8

> [!NOTE]
> **.NET 8** has a new default port (`8080`) as compared to **.NET 5** (`80`). So whatever that specifies port `80` in the full course, may need to change to port `8080`. [See this](https://stackoverflow.com/a/77743735).

There is an issue when following along **Containerizing the Platform Service** at **[2:40:26](https://youtu.be/DgVjEo3OGBI?si=6h9rPU74tyPB8yw9)**, just before going into **Pushing to Docker Hub**. After starting the container, I am unable to access it via browser nor Postman. It may be due to me using **.NET 8** as opposed to **.NET 5**, which is the version Les Jackson uses for the full course video. 

Some findings:
* [Docker: ASP.NET Core 8.0 app not accessible outside container](https://stackoverflow.com/questions/78601206/docker-asp-net-core-8-0-app-not-accessible-outside-container)
* [Breaking change in .NET 8 means instructions to run docker container are wrong](https://github.com/dotnet/dotnet-docker/issues/5194)

These findings lead me to a [tutorial](https://dotnet.microsoft.com/en-us/learn/aspnet/microservice-tutorial/run-docker) by Microsoft. Just follow the format `docker run -it --rm -p 3000:8080 --name mymicroservicecontainer mymicroservice` and it should be okay. To make it easier to follow along with the full course, change the port from `3000:8080` to `8080:8080`.

### Adding .dockerignore

The creation of **.dockerignore** is not part of the full course video. It is taken from a [tutorial](https://dotnet.microsoft.com/en-us/learn/aspnet/microservice-tutorial/docker-file) by Microsoft. I find it useful.

### Kubernetes command cheat sheet

> [!NOTE]
> Before executing a Kubernetes command, make sure your command prompt directory has been changed to where the **platforms-depl.yaml** and **platforms-np-srv.yaml** are located.

* To check if Kubernetes is running: `kubectl version`.
* To apply a Deployment file: `kubectl apply -f platforms-depl.yaml`.
  - This will create a container.
* To delete a deployment: `kubectl delete deployment platforms-depl`.
  - This will delete the container.
* To apply a Service file: `kubectl apply -f platforms-np-srv.yaml`.
* To delete a service: `kubectl delete service platformnpsrv-service`.
* To get all: `kubectl get all`.
* To get list of nodes: `kubectl get nodes`.
* To get list of deployments: `kubectl get deployments`.
* To get list of pods: `kubectl get pods`.
* To get list of services: `kubectl get services`.
* To get logs of a pod: `kubectl logs app-pod`.

## Part 4 - Starting Our 2nd Service

> [!NOTE]
> In the full course video, Les Jackson points out to change the port in **launchSettings.json** for the **CommandsService** project as it is using the same port number with **PlatformService** project. However, this is not the case for me. I assume in **.NET 8**, every project will use a different port number upon creation. In **.NET 5**, I assume every project in a solution uses the same port.

