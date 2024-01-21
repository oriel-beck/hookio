export default async function getUser() {
    const currentUser = await fetch("/api/users/current");
    return currentUser.json();
}