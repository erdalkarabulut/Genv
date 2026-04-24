import { type InputHTMLAttributes, forwardRef } from "react";
import { cn } from "@/lib/utils";

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  hint?: string;
  error?: string;
}

export const Input = forwardRef<HTMLInputElement, Props>(function Input(
  { label, hint, error, className, id, ...rest },
  ref,
) {
  const inputId = id ?? rest.name;
  return (
    <div>
      {label && (
        <label htmlFor={inputId} className="label">
          {label}
        </label>
      )}
      <input
        ref={ref}
        id={inputId}
        className={cn("input", error && "border-rose-500/60 focus:ring-rose-500/30", className)}
        {...rest}
      />
      {hint && !error && <p className="mt-1 text-[11px] text-ink-dim">{hint}</p>}
      {error && <p className="mt-1 text-[11px] text-accent-rose">{error}</p>}
    </div>
  );
});

interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  hint?: string;
  options?: { value: string; label: string }[];
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, hint, options, className, id, children, ...rest },
  ref,
) {
  const inputId = id ?? rest.name;
  return (
    <div>
      {label && (
        <label htmlFor={inputId} className="label">
          {label}
        </label>
      )}
      <select ref={ref} id={inputId} className={cn("input pr-8", className)} {...rest}>
        {options
          ? options.map((o) => (
              <option key={o.value} value={o.value} className="bg-bg-card">
                {o.label}
              </option>
            ))
          : children}
      </select>
      {hint && <p className="mt-1 text-[11px] text-ink-dim">{hint}</p>}
    </div>
  );
});
