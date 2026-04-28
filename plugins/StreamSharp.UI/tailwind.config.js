/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: '#FF8552',
        secondary: '#FF5D36',
        accent: '#72E6C8',
        background: '#08121C',
        backgroundDark: '#040B14',
        panel: '#132235',
        textPrimary: '#EDF4FF',
        textSecondary: '#C7D4E5',
        textMuted: '#90A4BB',
        success: '#72E6C8',
        warning: '#FFB762',
        error: '#FF6B7D',
        info: '#68C8FF',
      },
      boxShadow: {
        card: '0 22px 60px rgba(0, 0, 0, 0.3)',
        warm: '0 18px 40px rgba(255, 93, 54, 0.2)',
      },
      borderRadius: {
        card: '24px',
        control: '999px',
      },
      fontFamily: {
        sans: ['Manrope', 'system-ui', 'sans-serif'],
        display: ['Space Grotesk', 'Manrope', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
