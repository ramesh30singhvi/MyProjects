import { RouteInfo } from './horizontal-sidebar.metadata';

export const ROUTES: RouteInfo[] = [
    {
        path: '',
        title: 'Home',
        icon: 'home',
        class: '',
        ddclass: '',
        extralink: false,
        submenu: []
    },
    {
        path: '',
        title: 'Components',
        icon: 'grid',
        class: 'has-arrow',
        ddclass: 'mega-dropdown',
        extralink: false,
        submenu: [
            {
                path: '/component/accordion',
                title: 'Accordion',
                icon: 'mdi mdi-equal',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/alert',
                title: 'Alert',
                icon: 'mdi mdi-message-bulleted',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/carousel',
                title: 'Carousel',
                icon: 'mdi mdi-view-carousel',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/dropdown',
                title: 'Dropdown',
                icon: 'mdi mdi-arrange-bring-to-front',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/modal',
                title: 'Modal',
                icon: 'mdi mdi-tablet',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/pagination',
                title: 'Pagination',
                icon: 'mdi mdi-backburger',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/poptool',
                title: 'Popover & Tooltip',
                icon: 'mdi mdi-image-filter-vintage',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/progressbar',
                title: 'Progressbar',
                icon: 'mdi mdi-poll',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/rating',
                title: 'Ratings',
                icon: 'mdi mdi-bandcamp',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/tabs',
                title: 'Tabs',
                icon: 'mdi mdi-sort-variant',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/timepicker',
                title: 'Timepicker',
                icon: 'mdi mdi-calendar-clock',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/buttons',
                title: 'Button',
                icon: 'mdi mdi-toggle-switch',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            },
            {
                path: '/component/notifier',
                title: 'Notifier',
                icon: 'mdi mdi-bandcamp',
                class: '',
                ddclass: '',
                extralink: false,
                submenu: []
            }
        ]
    }
];
