# .NET Microservices - Full Course

A follow-along learning with [Les Jackson's .NET Microservices - Full Course video on YouTube](https://www.youtube.com/watch?v=DgVjEo3OGBI).

There is no Part 1 as it is introduction and theory only. You need to watch the video to know what this full course is about.

## Part 2 - Building The First Service

Some differences compared to the full course video:
1. Full course uses **.NET 5**.
   - I am using **.NET 8**.
2. Full course installs `AutoMapper.Extensions.Microsoft.DependencyInjection`.
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
  - If there's a NodePort-typed service, communicate by checking the assigned port.
    - `8080:32626/TCP` means that you can communicate with the Platforms API Controller route with `http://localhost:32626/api/platforms`.
* To get logs of a pod: `kubectl logs app-pod`.
* To restart a deployment: `kubectl rollout restart deployment platforms-depl`.
* To get list of namespaces: `kubectl get namespace`.
* To get list of pods for a namespace: `kubectl get pods --namespace=ingress-nginx`.
* To get list of storage class: `kubectl get storageclass`.

## Part 4 - Starting Our 2nd Service

> [!NOTE]
> In the full course video, Les Jackson points out to change the port in **launchSettings.json** for the **CommandsService** project as it is using the same port number with **PlatformService** project. However, this is not the case for me. I assume in **.NET 8**, every project will use a different port number upon creation. In **.NET 5**, I assume every project in a solution uses the same port.

### Adding a HTTP Client

- Two scenarios to try:
   1. Start both **PlatformService** and **CommandsService**.
   2. Start only **PlatformService**.
- Send a POST request to **PlatformService** controller.
- Check console and see what message is printed out for both scenarios.
  - You will notice the time taken to complete the request for the 2nd scenario is longer, indicating that this method is synchronous.

### Deploying service to Kubernetes

> [!NOTE]
> At this point, we are still using Node Port to communicate with the services.

Build, run and push the **CommandService** to your Docker Hub. Must be within **CommandsService** project directory.

1. `docker build -t <your_docker_hub_id>/commandservice .`.
2. `docker run -p 8080:8080 <your_docker_hub_id>/commandservice`.
3. `docker push <your_docker_hub_id>/commandservice`.

Deploy both services to Kubernetes. Must be within **K8S** directory.

1. `kubectl apply -f platforms-depl.yaml`.
2. Restart deployment to pull the latest version: `kubectl rollout restart deployment platforms-depl`.
3. `kubectl apply -f commands-depl.yaml`.

**HTTPS Redirection** is commented out in **Program.cs**.

### Adding an API Gateway

> [!NOTE]
> At this point, we moved away from using the Node Port and into using Ingress Nginx Controller to communicate with the services.

> [!WARNING]
> At the time of this making, when creating **ingress-srv.yaml**, it mentions that the annotation "kubernetes.io/ingress.class" is deprecated. Ignore this warning.

1. Following [this](https://kubernetes.github.io/ingress-nginx/deploy/#docker-desktop) guide.
2. `kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.12.0/deploy/static/provider/cloud/deploy.yaml`.
   - This will create the Ingress Nginx Controller container.
3. For **Windows**, go to `C:\Windows\System32\drivers\etc` and open the `hosts` file on a text editor and add in `127.0.0.1   acme.com`.
   - This is required because the host in **ingress-srv.yaml** is using **acme.com** to redirect to **localhost**.
4. `kubectl apply -f ingress-srv.yaml`.
   - All the port numbers uses `8080` instead of `80`.
   - Run the command on **Command Prompt** within **K8S** directory.
5. Test by sending request to `http://acme.com/api/platforms`.

## Part 5 - Starting with SQL Server

- `kubectl apply -f local-pvc.yaml` to create a **Persistent Volume Claim**.

### Adding a Kubernetes Secret

- `kubectl create secret generic mssql --from-literal=SA_PASSWORD="pa55w0rd!"`.
  - Remember the name `mssql` and the key `SA_PASSWORD` that is defined.
  - You can specify whatever password you want here. I'm only using what Les Jackson uses for an easier follow-along learning.
- If you're planning to use MSSQL 2022, use: `kubectl create secret generic mssql --from-literal=MSSQL_SA_PASSWORD="pa55w0rd!"`.

### Deploying SQL Server to Kubernetes

Some differences compared to the full course video:
1. Full course uses **MSSQL 2017**.
   - I am using **MSSQL 2022**.
2. Full course uses `SA_PASSWORD` as key because Les Jackson is using **MSSQL 2017**.
   - I am using `MSSQL_SA_PASSWORD` as key because I am using **MSSQL 2022**.

Apply the deployment with `kubectl apply -f mssql-plat-depl.yaml`.

### Accessing SQL Server via Management Studio

> [!NOTE]
> In the full course video, **SQL Server Management Studio (SSMS)** is used to connect into the server externally using the Load Balancer concept. However, I am using **SQL Server Object Explorer** on **Visual Studio 2022** to do so. If you have and wish to use **SSMS**, just follow the full course.

To test connect to the server on **Visual Studio 2022**:

1. **View** `>` **SQL Server Object Explorer**.
2. Click the **Add SQL Server** icon. Alternatively, right-click **SQL Server** and select **Add SQL Server**. Enter the connection detail:
   - Server Name: `localhost,1433`
   - Authentication: `SQL Server Authentication`
   - User Name: `sa`
   - Password: `pa55w0rd!` (or the one you specified when creating secret `mssql`)
   - Database Name: `<default>`
   - Encrypt: `Optional (False)`
   - Trust Server Certificate: `False`
3. Click **Connect** button.
4. **localhost,1433** should now be available in the tree under **SQL Server**.

To test that the persistent volume claim works:

1. Expand **localhost,1433**.
2. Right-click **Databases** and select **Add New Database**.
3. Name it anything, e.g. **Test**.
4. Right-click **localhost,1433** and select **Disconnect**.
5. Go to **Docker Desktop** `>` **Containers**.
6. Delete the **MSSQL** container.
   e.g. Find a container with name prefix `k8s_mssql_mssql-depl-<something-something>`.
7. A new **MSSQL** container should be running automatically.
8. Reconnect to the server on **SQL Server Object Explorer**.
9. Verify if **Test** database is still there.
   - If still there, then persistent volume claim is working as expected.
10. Delete **Test** database, since it is only created to verify persistent volume claim works.

### Updating our Platform Service to use SQL Server

1. In the full course video, the command to perform database migration is `dotnet ef migrations add initialmigration`.
   - This is not working for me which is probably due to some missing global settings.
   - I am using [`Add-Migration -Name InitialMigration`](https://learn.microsoft.com/en-us/ef/core/cli/powershell#add-migration) instead.
   - Before I can use `Add-Migration`, it requires me to install the NuGet package `Microsoft.EntityFrameworkCore.Tools`. At the time of this making, the latest version is `8.0.13`.
   - After installing, I close and reopen the solution.
   - I am also using the same `8.0.13` version for `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.InMemory` and `Microsoft.EntityFrameworkCore.SqlServer` NuGet packages.
2. In the full course video, there are no issues when redeploying **platforms-depl.yaml**.
   - For **.NET 8**, I get an exception with message `Only the invariant culture is supported in globalization-invariant mode`.
     - To fix this [issue](https://github.com/dotnet/SqlClient/issues/2239), I need to modify the value for `InvariantGlobalization` from `true` to `false`.
3. In the full course video, the connection string for **PlatformsConn** is `Server=mssql-clusterip-srv,1433;Initial Catalog=platformsdb;User ID=sa;Password=pa55w0rd!;`.
   - I am using `Server=10.105.207.125,1433;Database=platformsdb;User ID=sa;Password=pa55w0rd!;Encrypt=False;Trust Server Certificate=False;`.
   - For **.NET 8**, I get an exception with message `A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections.`. It fails to establish a connection when specifying `mssql-clusterip-srv,1433` as the server. I also try changing to `localhost,1433` but still getting the same exception message.
   - I run `kubectl get services` to check the **Cluster IP** value for both `mssql-clusterip-srv` (e.g. `10.105.207.125`) and `mssql-loadbalancer-srv` (e.g. `10.107.116.21`) services.
   - I modified the connection string to use `10.105.207.125,1433` as the server. It successfully connects to the SQL Server.
   - I modified the connection string to use `10.107.116.21,1433` as the server. It also successfully connects to the SQL Server.
   - I am assuming, the latest configuration is to use the **Cluster IP** instead of the service's name.

## Part 6 - Multi-Resource API

> [!NOTE]
> Nothing worth noting on this chapter.

## Part 7 - Message Bus and RabbitMQ

### RabbitMQ Overview

What is RabbitMQ
- A message broker - it accepts and forwards messages.
- Messages are sent by Producers (or Publishers).
- Messages are received by Consumers (or Subscribers).
- Messages are stored on Queues (essentially a message buffer).
  - This implementation is not part of the full course video.
- Exchanges can be used to add "routing" functionality.
- Uses Advanced Message Queueing Protocol (AMQP) and others.

4 Types of Exchange
- Direct Exchange
  - Delivers Messages to Queues based on a routing key.
  - Ideal for "direct" or unicast messaging.
- Fanout Exchange (The full course video uses this)
  - Delivers Messages to all Queues that are bound to the exchange.
  - It ignores the routing key.
  - Ideal for broadcast messages.
- Topic Exchange
  - Routes Messages to 1 or more Queues based on the routing key (and patterns).
  - Used for Multicast messaging.
  - Implements various Publisher / Subscriber Patterns.
- Header Exchange

### Deploy RabbitMQ to Kubernetes

> [!NOTE]
> The full course video uses RabbitMQ `3-management` version. I am using `4-management` version.

Login to RabbitMQ Management page on browser via `localhost:15672`. This is possible thanks to the Load Balancer.

## Part 8 - Asynchronous Messaging

### Add a Message Bus Publisher to Platform Service

1. The full course video uses RabbitMQ.Client version `6.2.2`.
   - I am using `7.1.0` version which is the latest version at the time of making this.
   - The implementation for `MessageBusClient` becomes different because some, if not all, of the methods are now asynchronous.
     - I refer to the code for [`EmitLog.cs`](https://www.rabbitmq.com/tutorials/tutorial-three-dotnet#putting-it-all-together) by RabbitMQ for the implementation while trying to make it similar to what the full course video does.
2. The full course video uses `rabbitmq-clusterip-srv` as the value for `RabbitMQHost` for **Production**.
   - I am using the **Cluster IP** specified on `kubectl get services` e.g. `10.108.210.180`.

### Command Service ground work

> [!NOTE]
> Nothing worth noting on this chapter.

### Event Processing

> [!NOTE]
> Nothing worth noting on this chapter.

### Adding an Event Listener

1. The full course video uses RabbitMQ.Client version `6.2.2`.
   - I am using `7.1.0` version which is the latest version at the time of making this.
   - The implementation for `MessageBusSubcriber` becomes different because some, if not all, of the methods are now asynchronous.
     - I refer to the code for [`ReceiveLogs.cs`](https://www.rabbitmq.com/tutorials/tutorial-three-dotnet#putting-it-all-together) by RabbitMQ for the implementation while trying to make it similar to what the full course video does.
2. The full course video uses `rabbitmq-clusterip-srv` as the value for `RabbitMQHost` for **Production**.
   - I am using the **Cluster IP** specified on `kubectl get services` e.g. `10.108.210.180`.
