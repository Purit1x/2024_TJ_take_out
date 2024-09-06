import {
    createRouter,
    createWebHashHistory
} from 'vue-router'

import Index from '@/views/Index.vue'
import Login from '@/views/Login.vue'
import NotFound from '@/views/404.vue'
import store from '@/store/index.js'
import UserHome from '@/views/user/UserHome.vue'
import MerchantHome from '@/views/Merchant/MerchantHome.vue'
import MerchantDish from '@/views/Merchant/MerchantDish.vue'
import MerchantPersonal from '@/views/Merchant/MerchantPersonal.vue'
import UserPersonal from '@/views/user/UserPersonal.vue'
import Menu from '@/views/user/Menu.vue'
import ShoppingCart from '@/views/user/ShoppingCart.vue'
import UserAddress from '@/views/user/UserAddress.vue'
import RiderHome from '@/views/rider/RiderHome.vue'
import RiderAssignment from '@/views/rider/RiderAssignment.vue'
import RiderInfo from '@/views/rider/RiderInfo.vue'
import RiderWage from '@/views/rider/RiderWage.vue'
import PlatformHome from '@/views/platform/PlatformHome.vue'
import MerchantManage from '@/views/platform/MerchantManage.vue'
import RiderManage from '@/views/platform/RiderManage.vue'
import OrderManage from '@/views/platform/OrderManage.vue'
import CouponManage from '@/views/platform/CouponManage.vue'
import CommentManage from '@/views/platform/CommentManage.vue'
import UserManage from '@/views/platform/UserManage.vue'
import StationManage from '@/views/platform/StationManage.vue'
import StatisticManage from '@/views/platform/StatisticManage.vue'
import UserCoupon from '@/views/user/UserCoupon.vue'
import CouponPurchase from '@/views/user/CouponPurchase.vue'
import SpecialOffer from '@/views/Merchant/SpecialOffer.vue'
import UserOrder from '@/views/user/UserOrder.vue'
import MyOrder from '@/views/user/MyOrder.vue'
import UserCartOrder from '@/views/user/UserCartOrder.vue'

// 默认路由，所有用户共享
const routers = [
    { path: "/", name: "index", component: Index }, // 添加name 是方便后续添加嵌套路由时方便
    { path: "/login", component: Login, meta: { title: "登录页面" } },
    { path: '/:pathMatch(.*)*', name: 'NotFound', component: NotFound },
    {
        path: "/merchant-home", component: MerchantHome, meta: { requiresAuth: true }, children: [
            {
                path: 'dish', // 子路由路径  
                component: MerchantDish, // 子组件  
            },
            {
                path: 'specialOffer',
                component: SpecialOffer,
            },
            {
                path: 'personal', // 新增路径  
                component: MerchantPersonal, // 新的子组件  
            },],
    },
    {
        path: "/user-home", component: UserHome, meta: { requiresAuth: true }, children: [
            {
                path: 'personal', //个人主页
                component: UserPersonal,
                children: [
                    {
                        path: 'coupon',
                        component: UserCoupon,
                        props: true,
                        children: [
                            {
                                path: 'couponPurchase',
                                component: CouponPurchase,
                                props: true,
                            },
                        ],
                    },
                    {
                        path: 'myOrder',
                        component: MyOrder,
                        props: true,
                    },
                ],
            },
            {
                path: 'merchant/:id',
                component: Menu, // 显示商家菜单的组件  
                props: true, // 允许将路由参数作为 props 传递  
                children: [
                    {
                        path: 'order', // 显示商家订单的组件  
                        component: UserOrder,
                        props: true,
                    },
                ],
            },
            {
                path: 'cart',
                component: ShoppingCart,
                props: true,
                children:[
                    {
                        path: 'cartorder',// 购物车结算页面
                        component: UserCartOrder,
                        meta: { hideParentContent: true }, // 子路由的元信息
                    },
                ]
            },
            {
                path: 'address',
                component: UserAddress,
            },
        ],
    },
    {
        path: "/rider-home", component: RiderHome, meta: { requiresAuth: true }, children: [
            {
                path: 'assignment', //订单
                component: RiderAssignment,
            },
            {
                path: 'information',
                component: RiderInfo,
            },
            {
                path: 'wage',
                component: RiderWage,
            }
        ],
    },
    {
        path: "/platform-home", component: PlatformHome, meta: { requiresAuth: true }, children: [
            {
                path: 'merchant-manage',
                component: MerchantManage,
            },
            {
                path: 'rider-manage',
                component: RiderManage,
            },
            {
                path: 'order-manage',
                component: OrderManage,
            },
            {
                path: 'coupon-manage',
                component: CouponManage,
            },
            {
                path: 'comment-manage',
                component: CommentManage,
            },
            {
                path: 'user-manage',
                component: UserManage,
            },
            {
                path: 'station-manage',
                component: StationManage,
            },
            {
                path: 'statistic-manage',
                component: StatisticManage,
            }
        ]
    },
]

// 动态路由，用于匹配菜单动态添加路由
const dynamicRouters = [{
    path: "/",
    name: "/",
    component: Index,
    meta: {
        title: "后台首页"
    }
}
]

// 创建router 实例, 并暴露
export const router = createRouter({
    history: createWebHashHistory(),
    routes: routers
})
// 路由守卫  

router.beforeEach((to, from, next) => {  
    const isAuthenticated = store.state.user || store.state.merchant ||store.state.rider||store.state.admin;  

    const requiresAuth = to.meta.requiresAuth; // 确保获取到目标路由的 requiresAuth 属性  
    if (requiresAuth && !isAuthenticated) {
        next({ path: '/login' }); // 重定向到登录  
    } else {
        next(); // 继续导航  
    }
});
// 动态添加路由的方法
export function addRoutes(menus) {
    // 是否有新的路由
    let hasNewRoute = false

    const findAndAddRoutesByMenus = (arr) => {
        arr.forEach(e => {
            let item = dynamicRouters.find(o => o.path == e.frontpath)
            // 是否已经存在路由里面，是否已经注册过
            if (item && !router.hasRoute(item.path)) {
                // 添加嵌套路由
                router.addRoute("admin", item)
                hasNewRoute = true

                // 该菜单下的子菜单
                if (item.children && item.children.length > 0) {
                    item.children.forEach(c => {
                        router.addRoute("admin", c)
                    })
                }
            }
            // 存在子菜单，递归调用
            if (e.child && e.child.length > 0) {
                findAndAddRoutesByMenus(e.child)
            }
        })
    }

    // 执行一次
    findAndAddRoutesByMenus(menus)

    // 打印路由集合
    // console.log(router.getRoutes())

    // 返回是否有新的路由
    return hasNewRoute
}