import { Navigate, useNavigate } from "react-router-dom";
import { useEffect } from "react";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth";
import { hasJwtRole } from "@/lib/jwt";

/** GenVApi JWT'de operation claim rolü **Admin** olmalı (seed kullanıcıya bağlı). */
export default function AdminRoute({ children }: { children: React.ReactNode }) {
  const { token } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!token) return;
    if (!hasJwtRole(token, "Admin")) {
      toast.error("Bu sayfa yalnızca yöneticiler içindir.");
      navigate("/", { replace: true });
    }
  }, [token, navigate]);

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (!hasJwtRole(token, "Admin")) {
    return null;
  }

  return <>{children}</>;
}
