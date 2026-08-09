# Details

- `Match` - where X = 'abc'
- `Prefix` - where X like 'abc%'
- `Range` - where between 0 and 10
- `Fuzzy match` - where X like '%abc'

- task_definition.json
```
{
    "taskDefinitionArn": "arn:aws:ecs:sa-east-1:939645320583:task-definition/search-microservice-task-definition:3",
    "containerDefinitions": [
        {
            "name": "search",
            "image": "939645320583.dkr.ecr.sa-east-1.amazonaws.com/mydockerimages",
            "cpu": 0,
            "portMappings": [
                {
                    "name": "search-80-tcp",
                    "containerPort": 80,
                    "hostPort": 80,
                    "protocol": "tcp",
                    "appProtocol": "http"
                }
            ],
            "essential": true,
            "environment": [
                {
                    "name": "host",
                    "value": "https://candidates.es.sa-east-1.aws.found.io"
                },
                {
                    "name": "password",
                    "value": "vdVSyerrS9Rbr3eynesw7b2Q"
                },
                {
                    "name": "userName",
                    "value": "elastic"
                },
                {
                    "name": "indexName",
                    "value": "index-candidate"
                }
            ],
            "environmentFiles": [],
            "mountPoints": [],
            "volumesFrom": [],
            "ulimits": [],
            "logConfiguration": {
                "logDriver": "awslogs",
                "options": {
                    "awslogs-create-group": "true",
                    "awslogs-group": "/ecs/",
                    "awslogs-region": "sa-east-1",
                    "awslogs-stream-prefix": "ecs"
                },
                "secretOptions": []
            },
            "healthCheck": {
                "command": [
                    "CMD-SHELL",
                    "curl -f http://localhost/search?city=SALVADOR&RATING=5 || exit 1"
                ],
                "interval": 30,
                "timeout": 5,
                "retries": 3
            }
        }
    ],
    "family": "search-microservice-task-definition",
    "taskRoleArn": "arn:aws:iam::939645320583:role/ecsTaskExecutionRole",
    "executionRoleArn": "arn:aws:iam::939645320583:role/ecsTaskExecutionRole",
    "networkMode": "awsvpc",
    "revision": 3,
    "volumes": [],
    "status": "ACTIVE",
    "requiresAttributes": [
        {
            "name": "com.amazonaws.ecs.capability.logging-driver.awslogs"
        },
        {
            "name": "com.amazonaws.ecs.capability.docker-remote-api.1.24"
        },
        {
            "name": "ecs.capability.execution-role-awslogs"
        },
        {
            "name": "com.amazonaws.ecs.capability.ecr-auth"
        },
        {
            "name": "com.amazonaws.ecs.capability.docker-remote-api.1.19"
        },
        {
            "name": "com.amazonaws.ecs.capability.task-iam-role"
        },
        {
            "name": "ecs.capability.container-health-check"
        },
        {
            "name": "ecs.capability.execution-role-ecr-pull"
        },
        {
            "name": "com.amazonaws.ecs.capability.docker-remote-api.1.18"
        },
        {
            "name": "ecs.capability.task-eni"
        },
        {
            "name": "com.amazonaws.ecs.capability.docker-remote-api.1.29"
        }
    ],
    "placementConstraints": [],
    "compatibilities": [
        "EC2",
        "FARGATE"
    ],
    "requiresCompatibilities": [
        "FARGATE"
    ],
    "cpu": "256",
    "memory": "512",
    "runtimePlatform": {
        "cpuArchitecture": "X86_64",
        "operatingSystemFamily": "LINUX"
    },
    "registeredAt": "2023-08-05T03:34:22.138Z",
    "registeredBy": "arn:aws:iam::939645320583:user/jeftegoesdev",
    "tags": []
}
```
