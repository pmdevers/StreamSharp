/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: '#E7A84B',
        secondary: '#D96A2B',
        accent: '#4FBF8A',
        background: '#FFF8EE',
        backgroundDark: '#3A2F2A',
        textPrimary: '#2B1F1A',
        textSecondary: '#6B5A52',
        success: '#4FBF8A',
        warning: '#E7A84B',
        error: '#C3424F',
        info: '#6EC9E8',
      },
      boxShadow: {
        card: '0 4px 12px rgba(0, 0, 0, 0.08)',
        warm: '0 12px 28px rgba(217, 106, 43, 0.22)',
      },
      borderRadius: {
        card: '12px',
        control: '8px',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        display: ['Poppins', 'Inter', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
