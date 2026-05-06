import { useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { Lock, Mail, ArrowRight, ShieldCheck } from "lucide-react";
import { motion } from "framer-motion";
import { toast } from "sonner";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { useAuth } from "@/lib/auth";
import { ThemeToggle } from "@/components/ThemeToggle";

interface Form {
  email: string;
  password: string;
}

export default function LoginPage() {
  const { token, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [loading, setLoading] = useState(false);
  const next = new URLSearchParams(location.search).get("next") ?? "/";
  const { register, handleSubmit, formState, setValue } = useForm<Form>({
    defaultValues: { email: "admin@genvapi.com", password: "Admin123!" },
  });

  if (token) return <Navigate to={next} replace />;

  const onSubmit = async (values: Form) => {
    setLoading(true);
    try {
      await login(values.email.trim(), values.password);
      toast.success("Giriş başarılı");
      navigate(next, { replace: true });
    } catch (err) {
      /* toast already shown by interceptor */
    } finally {
      setLoading(false);
    }
  };

  const fillDemo = () => {
    setValue("email", "admin@genvapi.com");
    setValue("password", "Admin123!");
  };

  return (
    <div className="min-h-screen relative overflow-hidden flex items-center justify-center p-6">
      <div className="absolute right-4 top-4 z-10">
        <ThemeToggle />
      </div>
      {/* Background glow */}
      <div className="pointer-events-none absolute inset-0 -z-10">
        <div className="absolute left-1/2 top-1/3 size-[720px] -translate-x-1/2 -translate-y-1/2 rounded-full bg-brand-500/20 blur-[120px]" />
        <div className="absolute right-10 bottom-10 size-[420px] rounded-full bg-emerald-500/10 blur-[100px]" />
        <div className="absolute left-10 top-10 size-[300px] rounded-full bg-sky-500/10 blur-[80px]" />
      </div>

      <div className="w-full max-w-5xl grid lg:grid-cols-2 gap-8 items-stretch">
        {/* Left: brand / marketing */}
        <motion.div
          initial={{ opacity: 0, x: -16 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.4 }}
          className="hidden lg:flex flex-col justify-between rounded-3xl border border-line/60 bg-bg-subtle/40 backdrop-blur-xl p-8"
        >
          <div className="flex items-center gap-3">
            <img src="/favicon.svg" alt="CryoFlow" className="size-11 rounded-2xl" />
            <div>
              <div className="text-lg font-semibold tracking-tight">CryoFlow</div>
              <div className="text-xs text-ink-dim">Hematoloji & Kök Hücre Operasyon Platformu</div>
            </div>
          </div>

          <div className="space-y-5">
            <h1 className="text-3xl font-semibold leading-tight tracking-tight">
              CD34/CD3 hesaplamadan
              <br />
              cryo yerleşimine kadar
              <span className="bg-gradient-to-r from-brand-400 to-emerald-300 bg-clip-text text-transparent">
                {" "}tek ekranda kontrol.
              </span>
            </h1>
            <ul className="space-y-3 text-sm text-ink-muted">
              <Feature>Gerçek zamanlı Cryo Grid (drag & drop ile torba taşıma)</Feature>
              <Feature>Otolog 4 gün / Allogeneik 2 gün aferez kuralları</Feature>
              <Feature>4 torbaya bölme + oto-yerleşim, tam audit log</Feature>
              <Feature>SignalR üzerinden anlık dashboard güncelleme</Feature>
            </ul>
          </div>

          <div className="flex items-center gap-2 text-[11px] text-ink-dim">
            <ShieldCheck className="size-3.5 text-accent-mint" />
            JWT oturumu · tenant izolasyonu · optimistic locking
          </div>
        </motion.div>

        {/* Right: form card */}
        <motion.div
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.4, delay: 0.05 }}
          className="rounded-3xl border border-line/60 bg-bg-card/70 backdrop-blur-xl p-7 sm:p-9 shadow-glow"
        >
          <div className="lg:hidden flex items-center gap-3 mb-6">
            <img src="/favicon.svg" alt="CryoFlow" className="size-9 rounded-xl" />
            <div>
              <div className="text-sm font-semibold">CryoFlow</div>
              <div className="text-[11px] text-ink-dim">Cryo & Apheresis</div>
            </div>
          </div>

          <h2 className="text-xl font-semibold tracking-tight">Giriş yap</h2>
          <p className="mt-1 text-sm text-ink-muted">
            Hesabınızla oturum açarak operasyon paneline devam edin.
          </p>

          <form className="mt-6 space-y-4" onSubmit={handleSubmit(onSubmit)}>
            <div>
              <label className="label">E-posta</label>
              <div className="relative">
                <Mail className="size-4 absolute left-3 top-1/2 -translate-y-1/2 text-ink-dim" />
                <input
                  type="email"
                  autoComplete="email"
                  className="input pl-9"
                  placeholder="admin@genvapi.com"
                  {...register("email", { required: true })}
                />
              </div>
              {formState.errors.email && (
                <p className="mt-1 text-[11px] text-accent-rose">E-posta zorunlu.</p>
              )}
            </div>

            <div>
              <label className="label">Şifre</label>
              <div className="relative">
                <Lock className="size-4 absolute left-3 top-1/2 -translate-y-1/2 text-ink-dim" />
                <input
                  type="password"
                  autoComplete="current-password"
                  className="input pl-9"
                  placeholder="••••••••"
                  {...register("password", { required: true, minLength: 4 })}
                />
              </div>
              {formState.errors.password && (
                <p className="mt-1 text-[11px] text-accent-rose">Şifre en az 4 karakter.</p>
              )}
            </div>

            <Button
              type="submit"
              loading={loading}
              className="w-full justify-center gap-2"
            >
              Giriş yap <ArrowRight className="size-4" />
            </Button>
          </form>

          <div className="mt-6 rounded-xl border border-line/60 bg-bg-elevated/40 p-3 text-xs text-ink-muted flex items-start gap-3">
            <ShieldCheck className="size-4 text-accent-mint mt-0.5 shrink-0" />
            <div className="flex-1">
              <div className="text-ink font-medium">Demo hesap</div>
              <div className="text-[11px]">
                <code className="text-ink-muted">admin@genvapi.com</code> /{" "}
                <code className="text-ink-muted">Admin123!</code>
              </div>
            </div>
            <button
              type="button"
              onClick={fillDemo}
              className="text-[11px] text-brand-400 hover:text-brand-300"
            >
              doldur
            </button>
          </div>
        </motion.div>
      </div>
    </div>
  );
}

function Feature({ children }: { children: React.ReactNode }) {
  return (
    <li className="flex items-start gap-2.5">
      <span className="mt-1.5 size-1.5 rounded-full bg-brand-400 shadow-[0_0_10px_rgba(56,189,248,0.6)]" />
      <span>{children}</span>
    </li>
  );
}
