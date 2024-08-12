import {
    createRouter,
    createWebHashHistory
} from 'vue-router'

import Index from '@/views/Index.vue'
import Login from '@/views/Login.vue'
import NotFound from '@/views/404.vue'
import store from '@/store/index.js'
import UserHome from '@/views/UserHome.vue'
import MerchantHome from '@/views/MerchantHome.vue'
import MerchantDish from '@/views/MerchantDish.vue'
import MerchantPersonal from '@/views/MerchantPersonal.vue'


// 默认路由，所有用户共享
const routers = [
    { path: "/", name: "index", component: Index }, // 添加name 是方便后续添加嵌套路由时方便
    { path: "/login", component: Login, meta: { title: "登录页面" } },
    { path: '/:pathMatch(.*)*', name: 'NotFound', component: NotFound },
    { path: "/merchant-home", component: MerchantHome, meta: { requiresAuth: true },children: [  
        {  
          path: 'dish', // 子路由路径  
          component: MerchantDish, // 子组件  
        },
        {  
            path: 'personal', // 新增路径  
            component: MerchantPersonal, // 新的子组件  
        },], 
    }, 
    { path: "/user-home", component: UserHome, meta: { requiresAuth: true } },
]

// 动态路由，用于匹配菜单动态添加路由
const dynamicRouters = [{
    path:"/",
    name:"/",
    component:Index,
    meta:{
        title:"后台首页"
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
    const isAuthenticated = store.state.user || store.state.merchant ||store.state.rider;  
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
                if(item.children && item.children.length>0){
                    item.children.forEach(c=>{
                        router.addRoute("admin", c)
                    })   
                }
            }
            // 存在子菜单，递归调用
            if(e.child && e.child.length>0){
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