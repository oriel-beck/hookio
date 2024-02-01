import { useEffect } from "react";
import { useNavigate, useOutletContext } from "react-router-dom";
import type { User } from "../types/types";

export default function LoginGuard({ children }: { children: JSX.Element }) {
    const user = useOutletContext() as User;
    const navigate = useNavigate();
    useEffect(() => {
        if (!user) return navigate("/", { replace: true });
    })
    return children;
}