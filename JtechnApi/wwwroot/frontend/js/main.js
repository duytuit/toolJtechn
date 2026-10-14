  const siteHeader = document.querySelector('#siteHeader');
        const setTheme = (dark) => {
            document.body.classList.toggle('theme-dark', dark);
            localStorage.setItem('northstar-theme', dark ? 'dark' : 'light');
            document.querySelectorAll('#themeToggle, #mobileThemeToggle').forEach(button => {
                button.textContent = dark ? '☀' : '☾';
                button.setAttribute('aria-label', dark ? 'Tắt giao diện tối' : 'Bật giao diện tối');
                button.setAttribute('title', dark ? 'Tắt giao diện tối' : 'Bật giao diện tối');
            });
        };
        setTheme(localStorage.getItem('northstar-theme') === 'dark');
        document.querySelectorAll('#themeToggle, #mobileThemeToggle').forEach(button => button.addEventListener('click', () => setTheme(!document.body.classList.contains('theme-dark'))));

        let lastScrollY = window.scrollY;
        window.addEventListener('scroll', () => {
            const currentScrollY = window.scrollY;
            siteHeader.classList.toggle('nav-scrolled', currentScrollY > 18);
            siteHeader.classList.toggle('nav-hidden', currentScrollY > lastScrollY && currentScrollY > 120);
            if (currentScrollY < lastScrollY || currentScrollY < 40) siteHeader.classList.remove('nav-hidden');
            lastScrollY = currentScrollY;
        }, { passive: true });

        const mobileMenuButton = document.querySelector('#mobileMenuButton');
        const mobileMenu = document.querySelector('#mobileMenu');
        const mobileMenuBackdrop = document.querySelector('#mobileMenuBackdrop');
        const mobileMenuClose = document.querySelector('#mobileMenuClose');
        const setMobileMenu = (open) => {
            mobileMenu.classList.toggle('is-open', open);
            mobileMenuBackdrop.classList.toggle('is-open', open);
            mobileMenuButton.setAttribute('aria-expanded', String(open));
            mobileMenu.setAttribute('aria-hidden', String(!open));
            mobileMenuBackdrop.setAttribute('aria-hidden', String(!open));
            document.body.classList.toggle('modal-open', open);
            if (open) siteHeader.classList.remove('nav-hidden');
        };
        mobileMenuButton?.addEventListener('click', () => {
            setMobileMenu(!mobileMenu.classList.contains('is-open'));
        });
        mobileMenuClose?.addEventListener('click', () => setMobileMenu(false));
        mobileMenuBackdrop?.addEventListener('click', () => setMobileMenu(false));
        mobileMenu?.querySelectorAll('a').forEach(link => link.addEventListener('click', () => setMobileMenu(false)));

        const blogModal = document.querySelector('#blogModal');
        const closeBlogModal = () => {
            blogModal.classList.add('hidden');
            blogModal.classList.remove('flex');
            document.body.classList.remove('modal-open');
        };
        document.querySelectorAll('.blog-card').forEach(card => card.addEventListener('click', () => {
            document.querySelector('#blogModalCategory').textContent = `${card.dataset.blogCategory} / Journal`;
            document.querySelector('#blogModalTitle').textContent = card.dataset.blogTitle;
            document.querySelector('#blogModalCopy').textContent = card.dataset.blogCopy;
            blogModal.classList.remove('hidden');
            blogModal.classList.add('flex');
            document.body.classList.add('modal-open');
        }));
        document.querySelector('#blogModalClose')?.addEventListener('click', closeBlogModal);
        blogModal?.addEventListener('click', event => { if (event.target === blogModal) closeBlogModal(); });
        document.addEventListener('keydown', event => { if (event.key === 'Escape') { closeBlogModal(); setMobileMenu(false); } });

        const featureTabs = document.querySelectorAll('[data-feature-tab]');
        const featurePanels = document.querySelectorAll('[data-feature-panel]');
        featureTabs.forEach(tab => tab.addEventListener('click', () => {
            const selectedFeature = tab.dataset.featureTab;
            featureTabs.forEach(item => {
                const isSelected = item === tab;
                item.classList.toggle('is-active', isSelected);
                item.setAttribute('aria-selected', String(isSelected));
            });
            featurePanels.forEach(panel => { panel.hidden = panel.dataset.featurePanel !== selectedFeature; });
        }));

        const revealObserver = new IntersectionObserver(entries => entries.forEach(entry => {
            if (entry.isIntersecting) { entry.target.classList.add('is-visible'); revealObserver.unobserve(entry.target); }
        }), { threshold: .12 });
        document.querySelectorAll('.reveal').forEach(element => revealObserver.observe(element));