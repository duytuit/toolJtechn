document.addEventListener('DOMContentLoaded', () => {
    const hero = document.querySelector('.ai-hero');
    if (!hero) return;

    const menuButton = hero.querySelector('.menu-toggle');
    const mobileNav = hero.querySelector('#mobileNav');
    const setMenuOpen = (open) => {
        menuButton?.setAttribute('aria-expanded', String(open));
        mobileNav?.classList.toggle('is-open', open);
        if (mobileNav) mobileNav.inert = !open;
        document.body.classList.toggle('menu-open', open);
    };

    menuButton?.addEventListener('click', () => {
        setMenuOpen(menuButton.getAttribute('aria-expanded') !== 'true');
    });
    mobileNav?.querySelectorAll('a').forEach((link) => {
        link.addEventListener('click', () => setMenuOpen(false));
    });
    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') setMenuOpen(false);
    });

    const gsap = window.gsap;
    if (!gsap) return;

    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const context = gsap.context(() => {
        if (window.ScrollTrigger) gsap.registerPlugin(window.ScrollTrigger);

        if (reducedMotion) return;

        const intro = gsap.timeline({ defaults: { ease: 'power3.out' } });
        intro
            .from('.hero-header', { y: -18, autoAlpha: 0, duration: .7 })
            .from('.hero-badge', { y: 14, autoAlpha: 0, duration: .5 }, '-=.28')
            .from('.title-word', { yPercent: 105, autoAlpha: 0, rotateX: -8, duration: .85, stagger: .11 }, '-=.2')
            .from('.hero-description', { y: 16, autoAlpha: 0, duration: .55 }, '-=.4')
            .from('.hero-actions .button', { y: 14, autoAlpha: 0, duration: .5, stagger: .1 }, '-=.3')
            .from('.stage-core-wrap', { scale: .78, autoAlpha: 0, duration: 1.05, ease: 'power2.out' }, '-=.62')
            .from('.stage-orbit, .orbit-point, .float-card, .float-chip', { scale: .84, autoAlpha: 0, duration: .7, stagger: .07 }, '-=.65')
            .from('.scroll-indicator', { y: 10, autoAlpha: 0, duration: .45 }, '-=.25');

        gsap.to('.stage-orbit--outer', { rotation: 14, duration: 26, repeat: -1, yoyo: true, ease: 'sine.inOut' });
        gsap.to('.stage-orbit--middle', { rotation: -13, duration: 32, repeat: -1, yoyo: true, ease: 'sine.inOut' });
        gsap.to('[data-float="1"]', { y: -11, rotation: -4, duration: 8, repeat: -1, yoyo: true, ease: 'sine.inOut' });
        gsap.to('[data-float="2"]', { y: 9, rotation: 3, duration: 10, repeat: -1, yoyo: true, ease: 'sine.inOut' });
        gsap.to('[data-float="3"]', { y: -8, duration: 12, repeat: -1, yoyo: true, ease: 'sine.inOut' });

        if (window.ScrollTrigger) {
            gsap.to('.stage-core-wrap', {
                scale: 1.22,
                y: 90,
                rotation: 12,
                ease: 'none',
                scrollTrigger: { trigger: hero, start: 'top top', end: 'bottom top', scrub: 1 }
            });
            gsap.to('.hero-copy', {
                y: -55,
                autoAlpha: .22,
                scale: .97,
                ease: 'none',
                scrollTrigger: { trigger: hero, start: 'top top', end: '65% top', scrub: 1 }
            });
            gsap.to('.hero-background', {
                scale: 1.1,
                yPercent: 5,
                ease: 'none',
                scrollTrigger: { trigger: hero, start: 'top top', end: 'bottom top', scrub: 1 }
            });
        }

        if (window.matchMedia('(hover: hover) and (pointer: fine)').matches) {
            const parallaxItems = gsap.utils.toArray('[data-parallax]').map((item) => ({
                item,
                depth: Number(item.dataset.parallax) || .1,
                x: gsap.quickTo(item, 'x', { duration: .8, ease: 'power2.out' }),
                y: gsap.quickTo(item, 'y', { duration: .8, ease: 'power2.out' })
            }));
            const buttonQuickTos = gsap.utils.toArray('.magnetic-button').map((button) => ({
                button,
                x: gsap.quickTo(button, 'x', { duration: .35, ease: 'power3.out' }),
                y: gsap.quickTo(button, 'y', { duration: .35, ease: 'power3.out' })
            }));

            hero.addEventListener('pointermove', (event) => {
                const bounds = hero.getBoundingClientRect();
                const pointerX = (event.clientX - bounds.left) / bounds.width - .5;
                const pointerY = (event.clientY - bounds.top) / bounds.height - .5;
                parallaxItems.forEach(({ item, depth, x, y }) => {
                    x(pointerX * depth * 42);
                    y(item.matches('.hero-glow, .stage-core') ? pointerY * depth * 34 : 0);
                });
            }, { passive: true });
            hero.addEventListener('pointerleave', () => {
                parallaxItems.forEach(({ x, y }) => { x(0); y(0); });
                buttonQuickTos.forEach(({ x, y }) => { x(0); y(0); });
            });

            buttonQuickTos.forEach(({ button, x, y }) => {
                button.addEventListener('pointermove', (event) => {
                    const bounds = button.getBoundingClientRect();
                    x(((event.clientX - bounds.left) / bounds.width - .5) * 16);
                    y(((event.clientY - bounds.top) / bounds.height - .5) * 12);
                });
                button.addEventListener('pointerleave', () => { x(0); y(0); });
            });
        }
    }, hero);

    window.addEventListener('pagehide', () => context.revert(), { once: true });
});