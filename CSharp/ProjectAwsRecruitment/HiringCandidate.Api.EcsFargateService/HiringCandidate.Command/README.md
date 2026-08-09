- task_definition.json
```
{
    "taskDefinitionArn": "arn:aws:ecs:sa-east-1:939645320583:task-definition/hiringcandidate-command:22",
    "containerDefinitions": [
        {
            "name": "hiringcandidate-command",
            "image": "939645320583.dkr.ecr.sa-east-1.amazonaws.com/hiringcandidate.command",
            "cpu": 0,
            "portMappings": [
                {
                    "name": "hiringcandidate-command-80-tcp",
                    "containerPort": 80,
                    "hostPort": 80,
                    "protocol": "tcp",
                    "appProtocol": "http"
                }
            ],
            "essential": true,
            "environment": [],
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
                    "curl -f http://localhost/health?param1=1&param2=2 || exit 1"
                ],
                "interval": 30,
                "timeout": 5,
                "retries": 3
            }
        }
    ],
    "family": "hiringcandidate-command",
    "taskRoleArn": "arn:aws:iam::939645320583:role/ecsTaskExecutionRole",
    "executionRoleArn": "arn:aws:iam::939645320583:role/ecsTaskExecutionRole",
    "networkMode": "awsvpc",
    "revision": 22,
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
    "cpu": "1024",
    "memory": "2048",
    "runtimePlatform": {
        "cpuArchitecture": "X86_64",
        "operatingSystemFamily": "LINUX"
    },
    "registeredAt": "2023-08-11T01:29:07.425Z",
    "registeredBy": "arn:aws:iam::939645320583:user/jeftegoesdev",
    "tags": []
}
```
