/*
 *   This content is licensed according to the W3C Software License at
 *   https://www.w3.org/Consortium/Legal/2015/copyright-software-and-document
 *
 *   File:   menu-button-actions.js
 *
 *   Desc:   Creates a menu button that opens a menu of actions
 */

'use strict';

class MenuActions {
    constructor(domNode, dotnetHelper) {
        this.domNode = domNode;
        this.dotnetHelper = dotnetHelper;
        this.buttonNode = domNode.querySelector('[data-role="menubutton"]');
        this.menuNode = domNode.querySelector('[role="menu"]');
        this.menuitemNodes = [];
        this.firstMenuitem = false;
        this.lastMenuitem = false;
        this.currentTarget = false;

        this.onButtonKeydownFunction = this.onButtonKeydown.bind(this);
        this.onWindowClickFunction = this.onWindowClick.bind(this);
        this.onMenuButtonClickFunction = this.onMenuButtonClick.bind(this);

        if (this.buttonNode) {
            this.buttonNode.addEventListener('keydown', this.onButtonKeydownFunction);
            window.addEventListener('click', this.onWindowClickFunction);
            this.buttonNode.addEventListener('click', this.onMenuButtonClickFunction);
        }

        this.updateMenuItemNodes();
    }

    removeEventListeners() {
        if (this.buttonNode) {
            this.buttonNode.removeEventListener('keydown', this.onButtonKeydownFunction);
            window.removeEventListener('click', this.onWindowClickFunction);
            this.buttonNode.removeEventListener('click', this.onMenuButtonClickFunction);
        }
    }

    updateMenuItemNodes() {

        if (this.currentTarget)
            this.currentTarget.classList.remove("focus");

        this.firstMenuitem = false;
        this.lastMenuitem = false;
        this.menuitemNodes = [];

        var nodes = this.domNode.querySelectorAll('[role="menuitem"]');

        for (var i = 0; i < nodes.length; i++) {
            var menuitem = nodes[i];
            this.menuitemNodes.push(menuitem);
            menuitem.tabIndex = -1;

            if (!this.firstMenuitem) {
                this.firstMenuitem = menuitem;
            }
            this.lastMenuitem = menuitem;
        }

        this.currentTarget = this.firstMenuitem;
    }

    openMenu() {
        $(this.menuNode).slideToggle();
        this.buttonNode.setAttribute('aria-expanded', 'true');
    }

    closeMenu() {
        $(this.menuNode).slideUp();
        this.buttonNode.setAttribute('aria-expanded', 'false');
    }

    performMenuAction(menuItem) {
        this.dotnetHelper.invokeMethodAsync('SelectInvokable', menuItem.getAttribute("key"));
    }

    setFocusToMenuitem(newMenuitem) {
        if (this.currentTarget)
            this.currentTarget.classList.remove("focus");

        this.currentTarget = newMenuitem;
        this.menuitemNodes.forEach(function (item) {
            if (item === newMenuitem) {
                item.tabIndex = 0;
                newMenuitem.classList.add("focus");
            } else {
                item.tabIndex = -1;
            }
        });

        $(this.menuNode).scrollTo(newMenuitem, 300);
    }

    setFocusToFirstMenuitem() {
        this.setFocusToMenuitem(this.firstMenuitem);
    }

    setFocusToLastMenuitem() {
        this.setFocusToMenuitem(this.lastMenuitem);
    }

    setFocusToPreviousMenuitem(currentMenuitem) {
        var newMenuitem, index;

        if (currentMenuitem === this.firstMenuitem) {
            newMenuitem = this.lastMenuitem;
        } else {
            index = this.menuitemNodes.indexOf(currentMenuitem);
            newMenuitem = this.menuitemNodes[index - 1];
        }

        this.setFocusToMenuitem(newMenuitem);

        return newMenuitem;
    }

    setFocusToNextMenuitem(currentMenuitem) {
        var newMenuitem, index;

        if (currentMenuitem === this.lastMenuitem) {
            newMenuitem = this.firstMenuitem;
        } else {
            index = this.menuitemNodes.indexOf(currentMenuitem);
            newMenuitem = this.menuitemNodes[index + 1];
        }
        this.setFocusToMenuitem(newMenuitem);

        return newMenuitem;
    }

    isOpen() {
        return this.buttonNode.getAttribute('aria-expanded') === 'true';
    }

    onMenuButtonClick(event) {
        if (!this.isOpen()) {
            this.openMenu();
        }
    }

    onWindowClick(event) {
        var isDropDown = false;

        if (event.target.id == this.menuNode.id || event.target.id == this.buttonNode.id)
            isDropDown = true;

        var parents = $(event.target).parents();
        for (let i = 0; i < parents.length; i++) {
            if (parents[i].id == this.menuNode.id || parents[i].id == this.buttonNode.id)
                isDropDown = true;
        }

        if (!isDropDown)
            this.closeMenu();
    }

    // Menu event handlers
    onButtonKeydown(event) {
        this.onMenuitemKeydown(event, this.currentTarget);
    }

    onMenuitemKeydown(event, target = null) {

        var tgt = null,
            key = event.key,
            flag = false;
        if (target == null)
            tgt = event.currentTarget;
        else
            tgt = target;

        function isPrintableCharacter(str) {
            return str.length === 1 && str.match(/\S/);
        }

        switch (key) {
            case ' ':
            case 'Enter':
                if (!this.isOpen()) {
                    this.openMenu();
                }
                this.performMenuAction(tgt);
                this.buttonNode.focus();
                flag = true;
                break;

            case 'Esc':
            case 'Escape':
                this.domNode.focus();
                this.closeMenu();
                flag = true;
                break;

            case 'Up':
            case 'ArrowUp':
                if (!this.isOpen()) {
                    this.openMenu();
                }
                this.setFocusToPreviousMenuitem(tgt);
                flag = true;
                break;

            case 'ArrowDown':
            case 'Down':
                if (!this.isOpen()) {
                    this.openMenu();
                }
                this.setFocusToNextMenuitem(tgt);
                flag = true;
                break;

            case 'Home':
            case 'PageUp':
                if (!this.isOpen()) {
                    this.openMenu();
                }
                this.setFocusToFirstMenuitem();
                flag = true;
                break;

            case 'End':
            case 'PageDown':
                if (!this.isOpen()) {
                    this.openMenu();
                }
                this.setFocusToLastMenuitem();
                flag = true;
                break;

            case 'Delete':
            case 'Backspace':
                if (!this.isOpen()) {
                    this.openMenu();
                }
                break;


            default:
                if (isPrintableCharacter(key)) {
                    if (!this.isOpen()) {
                        this.openMenu();
                    }
                }
                break;
        }

        if (flag) {
            event.stopPropagation();
            event.preventDefault();
        }
    }
}

var menuActions = [];
// Initialize menu buttons
function initializeMenuActions(dotnetHelper, id, element) {
    if (menuActions[id]) {
        menuActions[id].removeEventListeners();
    }

    this.menuActions[id] = new MenuActions(element, dotnetHelper);
}

function updateMenuActions(id) {
    menuActions[id].updateMenuItemNodes();
}

jQuery.fn.scrollTo = function (elem, speed) {
    var $this = jQuery(this);
    var $this_top = $this.offset().top;
    var $this_bottom = $this_top + $this.height();
    var $elem = jQuery(elem);
    var $elem_top = $elem.offset().top;
    var $elem_bottom = $elem_top + $elem.height();

    if ($elem_top > $this_top && $elem_bottom < $this_bottom) {
        // in view so don't do anything
        return;
    }
    var new_scroll_top;
    if ($elem_top < $this_top) {
        new_scroll_top = { scrollTop: $this.scrollTop() - $this_top + $elem_top - 10 };
    } else {
        new_scroll_top = { scrollTop: $elem_bottom - $this_bottom + $this.scrollTop() + 10 };
    }
    $this.finish().animate(new_scroll_top, speed === undefined ? 100 : speed);
    return this;
};