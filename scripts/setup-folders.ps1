# PowerShell script to create the full directory structure
# Run this on the Windows dev machine or VPS

$platformRoot = "/opt/platform"

$folders = @(
    "apps/orghub/orghubdev",
    "apps/orghub/orghub_hekzor_live",
    "apps/orghub/orghub_cfl_dev",
    "apps/orghub/orghub_cfl_live",
    "apps/orghub/orghub_cfl_uat",
    "apps/orghub/orghub_sdc_live",
    "apps/orghub/orghub_sdc_uat",
    "apps/ustock",
    "apps/eims",
    "apps/unitymicrofund",
    "docker/apps/orghub/orghubdev",
    "docker/apps/orghub/orghub_hekzor_live",
    "docker/apps/orghub/orghub_cfl_dev",
    "docker/apps/orghub/orghub_cfl_live",
    "docker/apps/orghub/orghub_cfl_uat",
    "docker/apps/orghub/orghub_sdc_live",
    "docker/apps/orghub/orghub_sdc_uat",
    "docker/apps/ustock",
    "docker/apps/eims",
    "docker/apps/unitymicrofund",
    "docker/databases/postgres",
    "docker/monitoring",
    "nginx",
    "backups",
    "logs",
    "scripts",
    "docs"
)

foreach ($folder in $folders) {
    $path = Join-Path $platformRoot $folder
    New-Item -ItemType Directory -Path $path -Force | Out-Null
    Write-Host "Created: $path"
}

Write-Host "`nDirectory structure created successfully under $platformRoot"
