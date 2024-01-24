export function getAuth() {
    const auth = document.cookie
        .split("; ")
        .find((row) => row.startsWith("Authorization="))
        ?.split("=")[1];
    return auth;
}