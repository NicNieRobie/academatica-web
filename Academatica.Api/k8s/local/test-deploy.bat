helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
helm install redis --set persistence.storageClass=nfs-client,redis.replicas.persistence.storageClass=nfs-client,auth.password=redis bitnami/redis --set volumePermissions.enabled=true
helm install rabbitmq --set auth.username=rabbitmq,auth.password=rabbitmq bitnami/rabbitmq
helm install academaticaapiauth ./academaticaapiauth
helm install academaticaapiusers ./academaticaapiusers
helm install academaticaapicourse ./academaticaapicourse
helm install academaticaapileaderboards ./academaticaapileaderboards
helm install academaticaweb ./academaticaweb
helm upgrade --install ingress-nginx ingress-nginx --repo https://kubernetes.github.io/ingress-nginx --namespace ingress-nginx --create-namespace
kubectl apply -f ingress-acme-srv.yaml
