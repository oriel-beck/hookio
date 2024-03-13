import flattenPalette from "tailwindcss/lib/util/flattenColorPalette";
import aspectRatio from "@tailwindcss/aspect-ratio"

const {
  default: flattenColorPalette,
} = flattenPalette;

/** @type {import('tailwindcss').Config} */
export default {
  content: ["./src/**/*.{ts,tsx}"],
  darkMode: "class",
  theme: {},
  plugins: [aspectRatio, addVariablesForColors],
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function addVariablesForColors({ addBase, theme }: any) {
  const allColors = flattenColorPalette(theme("colors"));
  const newVars = Object.fromEntries(
    Object.entries(allColors).map(([key, val]) => [`--${key}`, val])
  );

  addBase({
    ":root": newVars,
  });
}