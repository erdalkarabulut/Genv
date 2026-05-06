import { useState, useMemo } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Users } from "@/lib/api";
import { Card, CardHeader } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Badge } from "@/components/ui/Badge";
import { Modal } from "@/components/ui/Modal";
import { Plus, Shield, UserCheck, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

interface OperationClaim {
  id: number;
  name: string;
}

interface UserItem {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  status: boolean;
}

export default function UsersPage() {
  const qc = useQueryClient();

  const [page, setPage] = useState(0);
  const [showModal, setShowModal] = useState(false);
  const [selectedClaims, setSelectedClaims] = useState<number[]>([]);

  // Form state
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const usersQ = useQuery({
    queryKey: ["users", page],
    queryFn: () => Users.list(page, 20),
  });

  const claimsQ = useQuery({
    queryKey: ["users", "claims"],
    queryFn: () => Users.claims(0, 500),
  });

  const createMut = useMutation({
    mutationFn: () =>
      Users.createAdmin({
        firstName,
        lastName,
        email,
        password,
        operationClaimIds: selectedClaims,
      }),
    onSuccess: (data) => {
      toast.success(
        <div>
          <div className="font-semibold">{data.firstName} {data.lastName} oluşturuldu!</div>
          <div className="text-xs opacity-80">
            Yetkiler: {data.operationClaimNames.join(", ") || "Varsayılan"}
          </div>
        </div>
      );
      qc.invalidateQueries({ queryKey: ["users"] });
      setShowModal(false);
      resetForm();
    },
    onError: (err: any) => {
      toast.error(err?.response?.data?.message || err?.message || "Kullanıcı oluşturulurken hata.");
    },
  });

  const resetForm = () => {
    setFirstName("");
    setLastName("");
    setEmail("");
    setPassword("");
    setSelectedClaims([]);
  };

  const toggleClaim = (claimId: number) => {
    setSelectedClaims((prev) =>
      prev.includes(claimId) ? prev.filter((id) => id !== claimId) : [...prev, claimId]
    );
  };

  const totalPages = usersQ.data?.pages ?? 1;
  const totalCount = usersQ.data?.count ?? 0;

  const inputClass =
    "input w-full text-sm [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none";

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Kullanıcı Yönetimi</h1>
          <p className="text-sm text-ink-dim mt-1">
            Yeni kullanıcı oluştur ve yetkilendir.
          </p>
        </div>
        <Button icon={<Plus className="size-4" />} onClick={() => setShowModal(true)}>
          Yeni Kullanıcı
        </Button>
      </div>

      <Card>
        <CardHeader title="Kullanıcılar" subtitle={`${totalCount} kullanıcı`} />
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-line/60 text-left">
                <th className="px-4 py-3 text-xs font-medium text-ink-dim uppercase tracking-wide">Ad Soyad</th>
                <th className="px-4 py-3 text-xs font-medium text-ink-dim uppercase tracking-wide">E-posta</th>
                <th className="px-4 py-3 text-xs font-medium text-ink-dim uppercase tracking-wide">Durum</th>
              </tr>
            </thead>
            <tbody>
              {usersQ.isLoading ? (
                <tr>
                  <td colSpan={3} className="px-4 py-8 text-center text-ink-dim">
                    Yükleniyor...
                  </td>
                </tr>
              ) : usersQ.data?.items?.length === 0 ? (
                <tr>
                  <td colSpan={3} className="px-4 py-8 text-center text-ink-dim">
                    Henüz kullanıcı yok.
                  </td>
                </tr>
              ) : (
                usersQ.data?.items?.map((user: UserItem) => (
                  <tr key={user.id} className="border-b border-line/40 hover:bg-bg-elevated/30">
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <div className="size-8 rounded-full bg-brand-500/10 flex items-center justify-center">
                          <UserCheck className="size-4 text-brand-400" />
                        </div>
                        <span className="font-medium">{user.firstName} {user.lastName}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3 text-ink-muted">{user.email}</td>
                    <td className="px-4 py-3">
                      <Badge tone={user.status ? "mint" : "rose"}>
                        {user.status ? "Aktif" : "Pasif"}
                      </Badge>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t border-line/60">
            <p className="text-xs text-ink-dim">
              Sayfa {page + 1} / {totalPages} — {totalCount} kullanıcı
            </p>
            <div className="flex gap-2">
              <Button
                variant="soft"
                size="sm"
                disabled={page === 0}
                onClick={() => setPage((p) => Math.max(0, p - 1))}
              >
                Önceki
              </Button>
              <Button
                variant="soft"
                size="sm"
                disabled={page >= totalPages - 1}
                onClick={() => setPage((p) => Math.min(totalPages - 1, p + 1))}
              >
                Sonraki
              </Button>
            </div>
          </div>
        )}
      </Card>

      {/* Create Modal */}
      <Modal
        open={showModal}
        onClose={() => {
          setShowModal(false);
          resetForm();
        }}
        title="Yeni Kullanıcı Oluştur"
      >
        <form
          onSubmit={(e) => {
            e.preventDefault();
            if (!firstName.trim() || !lastName.trim() || !email.trim() || !password.trim()) {
              toast.error("Tüm alanları doldurun.");
              return;
            }
            createMut.mutate();
          }}
          className="space-y-4"
        >
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-ink-dim mb-1.5">Ad</label>
              <input
                type="text"
                className={inputClass}
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                placeholder="Örn: Ahmet"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-ink-dim mb-1.5">Soyad</label>
              <input
                type="text"
                className={inputClass}
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                placeholder="Örn: Yılmaz"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-dim mb-1.5">E-posta</label>
            <input
              type="email"
              className={inputClass}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="ornek@domain.com"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-dim mb-1.5">Şifre</label>
            <input
              type="password"
              className={inputClass}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Min. 4 karakter"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-dim mb-2">
              <div className="flex items-center gap-1.5">
                <Shield className="size-3.5 text-brand-400" />
                Yetkiler (opsiyonel)
              </div>
            </label>
            {claimsQ.isLoading ? (
              <div className="flex items-center gap-2 text-xs text-ink-dim py-2">
                <Loader2 className="size-3.5 animate-spin" />
                Yetkiler yükleniyor...
              </div>
            ) : (
              <div className="rounded-lg border border-line/60 bg-bg-subtle/50 p-3 max-h-48 overflow-y-auto">
                {claimsQ.data?.items?.map((claim: OperationClaim) => {
                  const isSelected = selectedClaims.includes(claim.id);
                  return (
                    <button
                      key={claim.id}
                      type="button"
                      onClick={() => toggleClaim(claim.id)}
                      className={cn(
                        "px-3 py-1.5 rounded-lg border text-xs font-medium transition",
                        isSelected
                          ? "border-brand-500/40 bg-brand-500/10 text-brand-400"
                          : "border-line/60 bg-bg-elevated/30 text-ink-dim hover:border-brand-500/20"
                      )}
                    >
                      {claim.name}
                    </button>
                  );
                })}
              </div>
            )}
            {selectedClaims.length > 0 && (
              <p className="mt-1.5 text-xs text-ink-dim">
                {selectedClaims.length} yetki seçildi
              </p>
            )}
          </div>

          <div className="flex justify-end gap-3 pt-2 border-t border-line/60">
            <Button
              variant="soft"
              type="button"
              onClick={() => {
                setShowModal(false);
                resetForm();
              }}
            >
              İptal
            </Button>
            <Button type="submit" disabled={createMut.isPending}>
              {createMut.isPending ? (
                <>
                  <Loader2 className="size-4 animate-spin mr-1" />
                  Oluşturuluyor...
                </>
              ) : (
                "Kullanıcı Oluştur"
              )}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
