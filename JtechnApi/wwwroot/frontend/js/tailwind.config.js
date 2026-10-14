window.tailwind = window.tailwind || {};
tailwind.config = {
    darkMode: 'class',
    theme: {
        extend: {
            fontFamily: {
                display: ['Space Grotesk', 'sans-serif'],
                sans: ['DM Sans', 'sans-serif']
            },
            colors: {
                ink: '#10202b',
                mint: '#b8f36b',
                aqua: '#8ee8db',
                cloud: '#f4f5ef'
            },
            boxShadow: {
                soft: '0 24px 70px rgba(16, 32, 43, .12)'
            }
        }
    }
};
