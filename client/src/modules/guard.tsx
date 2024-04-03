import { useEffect } from "react";
import { useNavigate, useOutletContext, useParams } from "react-router-dom";
import type { User } from "../types/types";

export default function LoginGuard({ children }: { children: JSX.Element }) {
    const user = useOutletContext() as User;
    const navigate = useNavigate();
    const params = useParams();
    useEffect(() => {
        if (!user) return navigate("/", { replace: true });
        if (params.serverId && user.guilds.findIndex((g) => g.id === params.serverId) === -1) return navigate("/servers", { replace: true })
    })
    return children;
}