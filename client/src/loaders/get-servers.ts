export default async function getServers() {
    const servers = await fetch("/api/users/servers")
    return servers.json();
}