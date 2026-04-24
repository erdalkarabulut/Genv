import { type ButtonHTMLAttributes, forwardRef } from "react";
import { cn } from "@/lib/utils";
import { Loader2 } from "lucide-react";

type Variant = "primary" | "soft" | "ghost" | "danger";
type Size = "sm" | "md" | "lg";

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  loading?: boolean;
  icon?: React.ReactNode;
}

const variantClass: Record<Variant, string> = {
  primary: "btn-primary",
  soft: "btn-soft",
  ghost: "btn-ghost",
  danger: "btn-danger",
};

const sizeClass: Record<Size, string> = {
  sm: "px-2.5 py-1.5 text-xs rounded-lg",
  md: "px-3.5 py-2 text-sm",
  lg: "px-4 py-2.5 text-sm",
};

export const Button = forwardRef<HTMLButtonElement, Props>(function Button(
  { className, variant = "primary", size = "md", loading, icon, children, disabled, ...rest },
  ref,
) {
  return (
    <button
      ref={ref}
      disabled={disabled || loading}
      className={cn(variantClass[variant], sizeClass[size], className)}
      {...rest}
    >
      {loading ? <Loader2 className="size-4 animate-spin" /> : icon}
      {children}
    </button>
  );
});
