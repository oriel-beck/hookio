import { useNavigate, useOutletContext } from "react-router-dom";
import { User } from "../types/types";
import { useEffect } from "react";

export default function Dashboard() {
    const user = useOutletContext() as User;
    const navigate = useNavigate();
    useEffect(() => {
        if (!user) return navigate("/", { replace: true });
    })
    return (
        <>
            This is the dashboard components
        </>
    )
}